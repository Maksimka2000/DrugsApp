using DrugsApp.Domain.Common;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.TempDrugFeature.CompleteRevert;
using DrugsApp.Features.TempDrugFeature.ProcessTempDrugRevertChunk;
using MediatR;
using System.Linq;
using ErrorOr;

namespace DrugsApp.Features.TempDrugFeature.ApplicationServices;

internal sealed class TempDrugRevertApplicationService(
    ITempDrugImportBatchRepository _batchRepository,
    ITempDrugRepository _tempDrugRepository,
    IMediator _mediator,
    IUnitOfWork _unitOfWork) : ITempDrugRevertApplicationService
{
    private const int ChunkSize = 100;

    public async Task RunRevertAsync(Guid batchId, CancellationToken cancellationToken)
    {
        try
        {
            // get batch
            TempDrugImportBatch? batch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
            if (batch is null || batch.StatusOfImport != ImportBatchStatus.RevertInProgress)
            {
                await MarkBatchErrorAsync(batchId, "Batch is not in progress or does not exist", cancellationToken);
                return;
            }

            // get temp drug ids
            List<Guid> tempDrugIds = await _tempDrugRepository.GetIdsByImportBatchIdAsync(batchId, cancellationToken);
            foreach (IEnumerable<Guid> chunk in tempDrugIds.Chunk(ChunkSize))
            {
                var chunkIds = chunk.ToList();
                // ProcessTempDrugRevertChunkCommand map to command 
                ProcessTempDrugRevertChunkCommand chunkCommand = chunkIds.MapToCommand(batchId);
                // ProcessTempDrugRevertChunkCommand call 
                ErrorOr<Success> chunkResult = await _mediator.Send(chunkCommand, cancellationToken);
                if (chunkResult.IsError)
                {
                    await MarkBatchErrorAsync(batchId, chunkResult.FirstError.Description, cancellationToken);
                    return;
                }
            }

            // get final batch and check if it is cancelled
            TempDrugImportBatch? finalBatch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
            if (finalBatch is null || finalBatch.StatusOfImport == ImportBatchStatus.Cancelled)
            {
                return;
            }

            // CompleteRevertCommand map to command 
            CompleteRevertCommand command = CompleteRevertMapping.MapToCommand(batchId);
            // CompleteRevertCommand call 
            ErrorOr<Success> completeRevertResult = await _mediator.Send(command, cancellationToken);
            
            //
            if (completeRevertResult.IsError)
            {
                await MarkBatchErrorAsync(batchId, completeRevertResult.FirstError.Description, cancellationToken);
                return;
            }
        }
        catch (Exception exception)
        {
            await MarkBatchErrorAsync(batchId, exception.Message, cancellationToken);
        }
    }

    private async Task MarkBatchErrorAsync(Guid batchId, string? errorNote, CancellationToken cancellationToken)
    {
        TempDrugImportBatch? batch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
        if (batch is null)
        {
            return;
        }

        batch.MarkError(errorNote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

