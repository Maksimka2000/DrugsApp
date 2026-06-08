using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SupportFeature.Entities;
using DrugsApp.Domain.SupportFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupportFeature.RemoveFaqEntry;

/// <summary>
/// Handles deletion of FAQ entries.
/// </summary>
internal sealed class RemoveFaqEntryHandler(IFaqEntryRepository _faqEntryRepository)
    : IRequestHandler<RemoveFaqEntryCommand, ErrorOr<Success>>
{
    /// <summary>
    /// Removes a FAQ entry when it exists.
    /// </summary>
    /// <param name="command">Delete command containing the FAQ entry identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when deletion is staged, or not-found errors otherwise.</returns>
    public async Task<ErrorOr<Success>> Handle(
        RemoveFaqEntryCommand command,
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

        _faqEntryRepository.Remove(faqEntry);

        return Result.Success;
    }
}
