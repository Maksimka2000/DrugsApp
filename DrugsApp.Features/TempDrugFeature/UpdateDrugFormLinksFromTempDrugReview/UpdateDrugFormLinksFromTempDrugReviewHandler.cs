using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview;

/// <summary>
/// Handles updating drug form links from a temporary drug review.
/// </summary>
/// <param name="_tempDrugReviewApplicationService">The temporary drug review application service.</param>
internal sealed class UpdateDrugFormLinksFromTempDrugReviewHandler(ITempDrugReviewApplicationService _tempDrugReviewApplicationService)
        : IRequestHandler<UpdateDrugFormLinksFromTempDrugReviewCommand, ErrorOr<UpdatedDrugFormLinksFromTempDrugReviewResponse>>
{

    /// <summary>
    /// Updates drug form links from a temporary drug review.
    /// </summary>
    /// <param name="command">The command to update drug form links from a temporary drug review.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated drug form links from a temporary drug review.</returns>
    public async Task<ErrorOr<UpdatedDrugFormLinksFromTempDrugReviewResponse>> Handle(
        UpdateDrugFormLinksFromTempDrugReviewCommand command,
        CancellationToken cancellationToken)
    {
        // Map the command to a draft
        UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceDraft tempDrugApplicationServiceDraft = command.MapToDraft();
        // Update the drug form links from the temporary drug review
        ErrorOr<UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult> result = await _tempDrugReviewApplicationService.UpdateDrugFormLinksAsync(tempDrugApplicationServiceDraft, cancellationToken);
        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.MapToResponse();
    }
}

