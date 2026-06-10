using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.UpsertAllergySummary;

/// <summary>
/// Handles upsert of allergy summary answers.
/// </summary>
internal sealed class UpsertAllergySummaryHandler(IClientAllergySummaryRepository _summaryRepository)
    : IRequestHandler<UpsertAllergySummaryCommand, ErrorOr<AllergySummaryResponse>>
{
    public async Task<ErrorOr<AllergySummaryResponse>> Handle(
        UpsertAllergySummaryCommand command,
        CancellationToken cancellationToken)
    {
        ClientAllergySummary? summary = await _summaryRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        bool isNew = summary is null;

        if (isNew)
        {
            ErrorOr<ClientAllergySummary> created = ClientAllergySummary.CreateForUser(command.UserId);
            if (created.IsError)
            {
                return created.Errors;
            }

            summary = created.Value;
        }

        AllergyAnswerState? severeReactionStatus =
            AllergySharedMappings.MapAnswerStateFromToken(command.Request.SevereReactionStatus);
        AllergyAnswerState? pollenStatus =
            AllergySharedMappings.MapAnswerStateFromToken(command.Request.PollenDustMitesAllergyStatus);
        AllergyAnswerState? insectStatus =
            AllergySharedMappings.MapAnswerStateFromToken(command.Request.InsectVenomAllergyStatus);
        AllergyAnswerState? latexStatus =
            AllergySharedMappings.MapAnswerStateFromToken(command.Request.LatexAllergyStatus);

        ErrorOr<Success> upsertResult = summary!.UpsertAnswers(
            severeReactionStatus,
            command.Request.CarriesAllergyMedication,
            pollenStatus,
            insectStatus,
            latexStatus);
        if (upsertResult.IsError)
        {
            return upsertResult.Errors;
        }

        if (isNew)
        {
            await _summaryRepository.AddAsync(summary, cancellationToken);
        }

        return summary.MapToSummaryResponse();
    }
}
