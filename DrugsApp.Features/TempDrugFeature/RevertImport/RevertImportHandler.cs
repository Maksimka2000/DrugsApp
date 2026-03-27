using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Interfaces;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.RevertImport;

/// <summary>
/// Handles revert import command for a selected batch.
/// </summary>
internal sealed class RevertImportHandler(
    ITempDrugImportBatchRepository _batchRepository,
    ITempDrugImportJobScheduler _tempDrugImportJobScheduler)
    : IRequestHandler<RevertImportCommand, ErrorOr<RevertImportResponse>>
{
    public async Task<ErrorOr<RevertImportResponse>> Handle(RevertImportCommand command, CancellationToken cancellationToken)
    {
        // get batch
        TempDrugImportBatch? batch = await _batchRepository.GetByIdAsync(command.BatchId, cancellationToken);
        if (batch is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found.",
                metadata: new()
                {
                    ["resourceType"] = "ImportBatch",
                    ["resourceId"] = command.BatchId.ToString()
                });
        }

        // start revert
        ErrorOr<Success> startRevertResult = batch.StartRevert();
        if (startRevertResult.IsError)
        {
            return startRevertResult.Errors;
        }

        // check if another import or revert is already in progress
        TempDrugImportBatch? activeImport = await _batchRepository.GetActiveImportAsync(cancellationToken);
        if (activeImport is not null && activeImport.Id != batch.Id)
        {
            return Error.Conflict(
                description: "Another import or revert is already in progress.",
                metadata: new()
                {
                    ["existingResourceId"] = activeImport.Id.ToString()
                });
        }

        // enqueue revert job
        string revertJobId = _tempDrugImportJobScheduler.EnqueueRevert(batch.Id);
        // set revert job id in the batch table 
        batch.SetRevertJobId(revertJobId);

        return batch.MapToResponse();
    }
}

