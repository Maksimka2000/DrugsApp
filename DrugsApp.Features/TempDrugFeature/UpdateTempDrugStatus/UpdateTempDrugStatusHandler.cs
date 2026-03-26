using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.UpdateTempDrugStatus;

/// <summary>
/// Handles updating the status of a temporary drug.
/// </summary>
internal sealed class UpdateTempDrugStatusHandler(ITempDrugRepository _tempDrugRepository)
        : IRequestHandler<UpdateTempDrugStatusCommand, ErrorOr<UpdatedTempDrugStatusResponse>>
{

    /// <summary>
    /// Loads a temporary drug, applies status change, and returns the updated state.
    /// </summary>
    /// <param name="command">Update status command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated status response.</returns>
    public async Task<ErrorOr<UpdatedTempDrugStatusResponse>> Handle(
        UpdateTempDrugStatusCommand command,
        CancellationToken cancellationToken)
    {
        // Early validation for appropriatee status. ReviewedAdded is not allowed to be set manually only by the system.
        if (command.Request.Status is TempDrugStatus.ReviewedAdded || command.Request.Status is TempDrugStatus.PendingReview)
        {
            return Error.Validation(
                description: "Status cannot be set to reviewed_added. ReviewedAdded is not allowed to be set manually only by the system");
        }

        // Load the temporary drug.
        TempDrug? tempDrug = await _tempDrugRepository.GetByIdAsync(command.TempDrugId, cancellationToken);
        if (tempDrug is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "TempDrug",
                    ["resourceId"] = command.TempDrugId.ToString()
                });
        }

        // Set the note if provided.
        if (command.Request.Note is not null)
        {
            ErrorOr<Success> setNoteResult = tempDrug.SetNote(command.Request.Note);
            if (setNoteResult.IsError)
            {
                return setNoteResult.Errors;
            }
        }

        // Change the status of the temporary drug.
        ErrorOr<Success> changeResult = tempDrug.ChangeStatus(command.Request.Status);
        if (changeResult.IsError)
        {
            return changeResult.Errors;
        }

        return tempDrug.MapToResponse();
    }
}


