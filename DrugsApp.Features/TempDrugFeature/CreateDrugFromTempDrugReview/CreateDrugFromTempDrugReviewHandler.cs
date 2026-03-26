using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.CreateDrugFromTempDrugReview;

/// <summary>
/// Handles creating a drug from a temporary drug review.
/// </summary>
/// <param name="_tempDrugReviewApplicationService"></param>
internal sealed class CreateDrugFromTempDrugReviewHandler(ITempDrugReviewApplicationService _tempDrugReviewApplicationService)
        : IRequestHandler<CreateDrugFromTempDrugReviewCommand, ErrorOr<CreatedDrugFromTempDrugReviewResponse>>
{

    /// <summary>
    /// Creates a drug from a temporary drug review.            
    /// </summary>
    /// <param name="command">The command to create a drug from a temporary drug review.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created drug from a temporary drug review response.</returns>
    public async Task<ErrorOr<CreatedDrugFromTempDrugReviewResponse>> Handle(
        CreateDrugFromTempDrugReviewCommand command,
        CancellationToken cancellationToken)
    {
        // map to the application service draft
        CreateDrugFromTempDrugReviewTempDrugApplicationServiceDraft tempDrugApplicationServiceDraft = command.MapToDraft();

        // create the drug from the temporary drug review service 
        ErrorOr<CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult> result = await _tempDrugReviewApplicationService.CreateDrugAsync(tempDrugApplicationServiceDraft, cancellationToken);
        if (result.IsError)
        {
            return result.Errors;
        }

        // map to the response
        return result.Value.MapToResponse();
    }
}

