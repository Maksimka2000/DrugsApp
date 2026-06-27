using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SupportFeature.Entities;
using DrugsApp.Domain.SupportFeature.Enums;
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

        if (command.Request.En?.Question is not null)
        {
            ErrorOr<Success> duplicateCheck = await EnsureNoDuplicateQuestionAsync(
                question: command.Request.En.Question,
                languageCode: FaqLanguageCode.En,
                excludedFaqEntryId: faqEntry.Id,
                cancellationToken: cancellationToken);

            if (duplicateCheck.IsError)
            {
                return duplicateCheck.Errors;
            }
        }

        if (command.Request.Pl?.Question is not null)
        {
            ErrorOr<Success> duplicateCheck = await EnsureNoDuplicateQuestionAsync(
                question: command.Request.Pl.Question,
                languageCode: FaqLanguageCode.Pl,
                excludedFaqEntryId: faqEntry.Id,
                cancellationToken: cancellationToken);

            if (duplicateCheck.IsError)
            {
                return duplicateCheck.Errors;
            }
        }

        if (command.Request.En is not null)
        {
            ErrorOr<Success> updateEnglishResult = faqEntry.UpdateEnglish(
                question: command.Request.En.Question,
                answer: command.Request.En.Answer);

            if (updateEnglishResult.IsError)
            {
                return updateEnglishResult.Errors;
            }
        }

        if (command.Request.Pl is not null)
        {
            ErrorOr<Success> updatePolishResult = faqEntry.UpdatePolish(
                question: command.Request.Pl.Question,
                answer: command.Request.Pl.Answer);

            if (updatePolishResult.IsError)
            {
                return updatePolishResult.Errors;
            }
        }

        AdminFaqEntryDto updatedFaqEntry = faqEntry.MapToAdminDto();
        return updatedFaqEntry.MapToResponse();
    }

    private async Task<ErrorOr<Success>> EnsureNoDuplicateQuestionAsync(
        string question,
        FaqLanguageCode languageCode,
        Guid excludedFaqEntryId,
        CancellationToken cancellationToken)
    {
        FaqEntry? duplicateFaqEntry = await _faqEntryRepository.GetByNormalizedQuestionAsync(
            question: question,
            languageCode: languageCode,
            cancellationToken: cancellationToken);

        if (duplicateFaqEntry is not null && duplicateFaqEntry.Id != excludedFaqEntryId)
        {
            return Error.Conflict(
                code: "FaqEntry.QuestionAlreadyExists",
                description: "A FAQ entry with the same question already exists.",
                metadata: new Dictionary<string, object>
                {
                    ["existingResourceId"] = duplicateFaqEntry.Id.ToString(),
                    ["languageCode"] = languageCode.ToString()
                });
        }

        return Result.Success;
    }
}
