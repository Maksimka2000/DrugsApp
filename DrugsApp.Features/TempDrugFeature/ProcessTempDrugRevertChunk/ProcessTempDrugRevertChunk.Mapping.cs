namespace DrugsApp.Features.TempDrugFeature.ProcessTempDrugRevertChunk;

internal static class ProcessTempDrugRevertChunkMapping
{
    public static ProcessTempDrugRevertChunkCommand MapToCommand(
        this List<Guid> tempDrugIds,
        Guid batchId) => new(
        BatchId: batchId,
        TempDrugIds: tempDrugIds);
}

