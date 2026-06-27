using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SupportFeature.Entities;
using DrugsApp.Domain.SupportFeature.Enums;
using DrugsApp.Domain.SupportFeature.Repositories;
using DrugsApp.Features.SupportFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupportFeature.AddFaqEntry;

/// <summary>
/// Handles creating FAQ entries with duplicate protection.
/// </summary>
internal sealed class AddFaqEntryHandler(IFaqEntryRepository _faqEntryRepository)
    : IRequestHandler<AddFaqEntryCommand, ErrorOr<CreatedFaqEntryResponse>>
{
    /// <summary>
    /// Creates a FAQ entry when no duplicate normalized question already exists.
    /// </summary>
    /// <param name="command">Create command containing FAQ payload data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created FAQ entry response or conflict/validation errors.</returns>
    public async Task<ErrorOr<CreatedFaqEntryResponse>> Handle(
        AddFaqEntryCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<Success> duplicateCheckResult = await EnsureNoDuplicateQuestionsAsync(
            englishQuestion: command.Request.En.Question,
            polishQuestion: command.Request.Pl.Question,
            excludedFaqEntryId: null,
            cancellationToken: cancellationToken);

        if (duplicateCheckResult.IsError)
        {
            return duplicateCheckResult.Errors;
        }

        ErrorOr<FaqEntry> faqEntryResult = FaqEntry.Create(
            englishQuestion: command.Request.En.Question,
            englishAnswer: command.Request.En.Answer,
            polishQuestion: command.Request.Pl.Question,
            polishAnswer: command.Request.Pl.Answer);

        if (faqEntryResult.IsError)
        {
            return faqEntryResult.Errors;
        }

        await _faqEntryRepository.AddAsync(faqEntryResult.Value, cancellationToken);

        AdminFaqEntryDto faqEntry = faqEntryResult.Value.MapToAdminDto();
        return faqEntry.MapToResponse();
    }

    private async Task<ErrorOr<Success>> EnsureNoDuplicateQuestionsAsync(
        string englishQuestion,
        string polishQuestion,
        Guid? excludedFaqEntryId,
        CancellationToken cancellationToken)
    {
        ErrorOr<Success> englishDuplicateCheck = await EnsureNoDuplicateQuestionAsync(
            question: englishQuestion,
            languageCode: FaqLanguageCode.En,
            excludedFaqEntryId: excludedFaqEntryId,
            cancellationToken: cancellationToken);

        if (englishDuplicateCheck.IsError)
        {
            return englishDuplicateCheck.Errors;
        }

        return await EnsureNoDuplicateQuestionAsync(
            question: polishQuestion,
            languageCode: FaqLanguageCode.Pl,
            excludedFaqEntryId: excludedFaqEntryId,
            cancellationToken: cancellationToken);
    }

    private async Task<ErrorOr<Success>> EnsureNoDuplicateQuestionAsync(
        string question,
        FaqLanguageCode languageCode,
        Guid? excludedFaqEntryId,
        CancellationToken cancellationToken)
    {
        FaqEntry? existingFaqEntry = await _faqEntryRepository.GetByNormalizedQuestionAsync(
            question: question,
            languageCode: languageCode,
            cancellationToken: cancellationToken);

        if (existingFaqEntry is not null && existingFaqEntry.Id != excludedFaqEntryId)
        {
            return Error.Conflict(
                code: "FaqEntry.QuestionAlreadyExists",
                description: "A FAQ entry with the same question already exists.",
                metadata: new Dictionary<string, object>
                {
                    ["existingResourceId"] = existingFaqEntry.Id.ToString(),
                    ["languageCode"] = languageCode.ToString()
                });
        }

        return Result.Success;
    }
}
