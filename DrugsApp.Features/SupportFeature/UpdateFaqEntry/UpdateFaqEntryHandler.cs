using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SupportFeature.Entities;
using DrugsApp.Domain.SupportFeature.Repositories;
using DrugsApp.Features.SupportFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupportFeature.UpdateFaqEntry;

/// <summary>
/// Handles tracked FAQ entry updates with duplicate question protection.
/// </summary>
internal sealed class UpdateFaqEntryHandler(IFaqEntryRepository _faqEntryRepository)
    : IRequestHandler<UpdateFaqEntryCommand, ErrorOr<UpdatedFaqEntryResponse>>
{
    /// <summary>
    /// Applies a partial update to a FAQ entry.
    /// </summary>
    /// <param name="command">Update command containing the target id and patch payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated FAQ response or not-found/conflict/validation errors.</returns>
    public async Task<ErrorOr<UpdatedFaqEntryResponse>> Handle(
        UpdateFaqEntryCommand command,
        CancellationToken cancellationToken)
    {
        FaqEntry? faqEntry = await _faqEntryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (faqEntry is null)
        {
            return Error.NotFound(
                code: "FaqEntry.NotFound",
                description: "The requested FAQ entry was not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "FaqEntry",
                    ["resourceId"] = command.Id.ToString()
                });
        }

        if (command.Request.Question is not null)
        {
            FaqEntry? duplicateFaqEntry = await _faqEntryRepository.GetByNormalizedQuestionAsync(
                question: command.Request.Question,
                cancellationToken: cancellationToken);

            if (duplicateFaqEntry is not null && duplicateFaqEntry.Id != faqEntry.Id)
            {
                return Error.Conflict(
                    code: "FaqEntry.QuestionAlreadyExists",
                    description: "A FAQ entry with the same question already exists.",
                    metadata: new Dictionary<string, object>
                    {
                        ["existingResourceId"] = duplicateFaqEntry.Id.ToString()
                    });
            }

            ErrorOr<Success> updateQuestionResult = faqEntry.UpdateQuestion(command.Request.Question);
            if (updateQuestionResult.IsError)
            {
                return updateQuestionResult.Errors;
            }
        }

        if (command.Request.Answer is not null)
        {
            ErrorOr<Success> updateAnswerResult = faqEntry.UpdateAnswer(command.Request.Answer);
            if (updateAnswerResult.IsError)
            {
                return updateAnswerResult.Errors;
            }
        }

        AdminFaqEntryDto updatedFaqEntry = faqEntry.MapToAdminDto();
        return updatedFaqEntry.MapToResponse();
    }
}
