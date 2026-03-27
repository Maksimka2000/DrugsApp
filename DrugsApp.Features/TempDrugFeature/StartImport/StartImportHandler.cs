using System.Text.Json;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Interfaces;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.StartImport;

/// <summary>
/// Handles start import command for a selected batch.
/// </summary>
internal sealed class StartImportHandler(
    ITempDrugImportBatchRepository _batchRepository,
    ITempDrugImportJobScheduler _tempDrugImportJobScheduler)
    : IRequestHandler<StartImportCommand, ErrorOr<StartImportResponse>>
{
    public async Task<ErrorOr<StartImportResponse>> Handle(StartImportCommand command, CancellationToken cancellationToken)
    {
        // Get the batch and check if it exists
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

        if (batch.StatusOfImport != ImportBatchStatus.NotStarted)
        {
            return Error.Conflict(
                description: "Import can be started only for not_started batches.",
                metadata: new()
                {
                    ["resourceId"] = batch.Id.ToString(),
                    ["currentStatus"] = batch.StatusOfImport.ToString()
                });
        }
 
        // Check if another import or revert is already in progress
        TempDrugImportBatch? activeImport = await _batchRepository.GetActiveImportAsync(cancellationToken);
        if (activeImport is not null)
        {
            return Error.Conflict(
                description: "Another import or revert is already in progress.",
                metadata: new()
                {
                    ["existingResourceId"] = activeImport.Id.ToString()
                });
        }

        // Serialize the mapping plan
        string mappingPlanJson = JsonSerializer.Serialize(command.Request.MappingPlan);
        // Enqueue the import job. Through the HangfireTempDrugImportJobScheduler then TempDrugImportHangfireJob then TempDrugImportApplicationService then ProcessTempDrugImportChunkHandler
        string hangfireJobId = _tempDrugImportJobScheduler.Enqueue(batch.Id);

        // Chnage data in the batch about the import
        ErrorOr<Success> startResult = batch.StartJob(hangfireJobId, mappingPlanJson, command.Request.Notes);
        if (startResult.IsError)
        {
            return startResult.Errors;
        }

        return batch.MapToResponse();
    }
}

