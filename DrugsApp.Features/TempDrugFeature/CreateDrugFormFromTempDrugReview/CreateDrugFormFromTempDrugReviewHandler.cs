using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.CreateDrugFormFromTempDrugReview;

/// <summary>
/// Handles creating a drug form from a temporary drug review.
/// </summary>
internal sealed class CreateDrugFormFromTempDrugReviewHandler(ITempDrugReviewApplicationService _tempDrugReviewApplicationService)
        : IRequestHandler<CreateDrugFormFromTempDrugReviewCommand, ErrorOr<CreatedDrugFormFromTempDrugReviewResponse>>
{

    /// <summary>
    /// Creates a drug form from a temporary drug review.
    /// </summary>
    /// <param name="command">The command to create a drug form from a temporary drug review.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created drug form from a temporary drug review response.</returns>
    public async Task<ErrorOr<CreatedDrugFormFromTempDrugReviewResponse>> Handle(
        CreateDrugFormFromTempDrugReviewCommand command,
        CancellationToken cancellationToken)
    {
        // map to the draft
        CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceDraft tempDrugApplicationServiceDraft = command.MapToDraft();
        // create the drug form from the temporary drug review service
        ErrorOr<CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult> result = await _tempDrugReviewApplicationService.CreateDrugFormAsync(tempDrugApplicationServiceDraft, cancellationToken);
        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.MapToResponse();
    }
}

