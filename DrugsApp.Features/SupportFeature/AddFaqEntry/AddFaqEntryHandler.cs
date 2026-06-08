using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SupportFeature.Entities;
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
        FaqEntry? existingFaqEntry = await _faqEntryRepository.GetByNormalizedQuestionAsync(
            question: command.Request.Question,
            cancellationToken: cancellationToken);

        if (existingFaqEntry is not null)
        {
            return Error.Conflict(
                code: "FaqEntry.QuestionAlreadyExists",
                description: "A FAQ entry with the same question already exists.",
                metadata: new Dictionary<string, object>
                {
                    ["existingResourceId"] = existingFaqEntry.Id.ToString()
                });
        }

        ErrorOr<FaqEntry> faqEntryResult = FaqEntry.Create(
            question: command.Request.Question,
            answer: command.Request.Answer);

        if (faqEntryResult.IsError)
        {
            return faqEntryResult.Errors;
        }

        await _faqEntryRepository.AddAsync(faqEntryResult.Value, cancellationToken);

        FaqEntryDto faqEntry = faqEntryResult.Value.MapToDto();
        return faqEntry.MapToResponse();
    }
}
