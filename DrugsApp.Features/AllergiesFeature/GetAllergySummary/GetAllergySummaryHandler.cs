using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.GetAllergySummary;

/// <summary>
/// Handles retrieval of allergy summary for the current user.
/// </summary>
internal sealed class GetAllergySummaryHandler(IClientAllergySummaryRepository _summaryRepository)
    : IRequestHandler<GetAllergySummaryQuery, ErrorOr<AllergySummaryResponse>>
{
    public async Task<ErrorOr<AllergySummaryResponse>> Handle(
        GetAllergySummaryQuery query,
        CancellationToken cancellationToken)
    {
        ClientAllergySummary? summary = await _summaryRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        if (summary is not null)
        {
            return summary.MapToSummaryResponse();
        }

        ErrorOr<ClientAllergySummary> created = ClientAllergySummary.CreateForUser(query.UserId);
        if (created.IsError)
        {
            return created.Errors;
        }

        return created.Value.MapToSummaryResponse();
    }
}
