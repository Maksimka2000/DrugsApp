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
internal sealed class UpdateTempDrugStatusHandler
    : IRequestHandler<UpdateTempDrugStatusCommand, ErrorOr<UpdatedTempDrugStatusResponse>>
{
    private readonly ITempDrugRepository _tempDrugRepository;

    public UpdateTempDrugStatusHandler(ITempDrugRepository tempDrugRepository)
    {
        _tempDrugRepository = tempDrugRepository;
    }

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

        ErrorOr<Success> changeResult = tempDrug.ChangeStatus(command.Request.Status);
        if (changeResult.IsError)
        {
            return changeResult.Errors;
        }

        return tempDrug.MapToResponse();
    }
}


