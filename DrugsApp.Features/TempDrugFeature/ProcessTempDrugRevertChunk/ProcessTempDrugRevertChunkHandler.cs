using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.ProcessTempDrugRevertChunk;

/// <summary>
/// Internal command for processing a single revert chunk.
/// </summary>
public sealed record ProcessTempDrugRevertChunkCommand(
    Guid BatchId,
    List<Guid> TempDrugIds) : IRequest<ErrorOr<Success>>;

/// <summary>
/// Handles per-chunk revert deletion workflow.
/// </summary>
internal sealed class ProcessTempDrugRevertChunkHandler(
    ITempDrugImportBatchRepository _batchRepository,
    ITempDrugRepository _tempDrugRepository)
    : IRequestHandler<ProcessTempDrugRevertChunkCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ProcessTempDrugRevertChunkCommand command, CancellationToken cancellationToken)
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

        // check if batch is cancelled
        if (batch.StatusOfImport == ImportBatchStatus.Cancelled)
        {
            return Result.Success;
        }

        // initialize lists for deleted and failed product identifiers
        var deletedProductIdentifiers = new List<string>();
        var failedDeleteProductIdentifiers = new List<string>();

        // process each temp drug id
        foreach (Guid tempDrugId in command.TempDrugIds)
        {
            // get temp drug
            TempDrug? tempDrug = await _tempDrugRepository.GetByIdAsync(tempDrugId, cancellationToken);
            if (tempDrug is null)
            {
                failedDeleteProductIdentifiers.Add(tempDrugId.ToString());
                continue;
            }

            // check if temp drug is pending review or review later
            if (tempDrug.Status is TempDrugStatus.PendingReview or TempDrugStatus.ReviewLater)
            {
                await _tempDrugRepository.DeleteAsync(tempDrugId, cancellationToken);
                deletedProductIdentifiers.Add(tempDrug.ProductIdentifier);
                continue;
            }

            // add product identifier to failed list
            failedDeleteProductIdentifiers.Add(tempDrug.ProductIdentifier);
        }

        // record chunk reverted
        batch.RecordChunkReverted(
            deletedCount: deletedProductIdentifiers.Count,
            deletedProductIdentifiers: deletedProductIdentifiers,
            failedDeleteProductIdentifiers: failedDeleteProductIdentifiers);

        return Result.Success;
    }
}

