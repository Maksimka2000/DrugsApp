using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Interfaces;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.CancelImport;

/// <summary>
/// Handles cancellation command for import/revert jobs.
/// </summary>
internal sealed class CancelImportHandler(
    ITempDrugImportBatchRepository _batchRepository,
    ITempDrugImportJobScheduler _tempDrugImportJobScheduler)
    : IRequestHandler<CancelImportCommand, ErrorOr<CancelImportResponse>>
{
    public async Task<ErrorOr<CancelImportResponse>> Handle(CancelImportCommand command, CancellationToken cancellationToken)
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

        // cancel batch
        ErrorOr<Success> cancelResult = batch.Cancel();
        if (cancelResult.IsError)
        {
            return cancelResult.Errors;
        }

        try
        {
            _tempDrugImportJobScheduler.Delete(batch.HangfireJobId);
        }
        catch
        {
            // Best-effort delete: batch-level cancellation status remains authoritative.
        }

        return batch.MapToResponse();
    }
}

