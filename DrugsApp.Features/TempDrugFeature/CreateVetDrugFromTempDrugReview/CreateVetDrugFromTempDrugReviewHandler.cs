using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.CreateVetDrugFromTempDrugReview;

/// <summary>
/// Handler for creating a veterinary drug from a temporary drug review.
/// </summary>
/// <param name="_tempDrugReviewApplicationService"></param>
internal sealed class CreateVetDrugFromTempDrugReviewHandler(ITempDrugReviewApplicationService _tempDrugReviewApplicationService)
        : IRequestHandler<CreateVetDrugFromTempDrugReviewCommand, ErrorOr<CreatedVetDrugFromTempDrugReviewResponse>>
{

/// <summary>
/// Handles creating a veterinary drug from a temporary drug review.
/// </summary>
/// <param name="command">The command to create a veterinary drug from a temporary drug review.</param>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>The created veterinary drug from a temporary drug review.</returns>
    public async Task<ErrorOr<CreatedVetDrugFromTempDrugReviewResponse>> Handle(
        CreateVetDrugFromTempDrugReviewCommand command,
        CancellationToken cancellationToken)
    {
        // Map the command to a draft
        CreateVetDrugFromTempDrugReviewDraft draft = command.MapToDraft();
        // Create the veterinary drug from the temporary drug review    
        ErrorOr<CreateVetDrugFromTempDrugReviewResult> result = await _tempDrugReviewApplicationService.CreateVetDrugAsync(draft, cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.MapToResponse();
    }
}

