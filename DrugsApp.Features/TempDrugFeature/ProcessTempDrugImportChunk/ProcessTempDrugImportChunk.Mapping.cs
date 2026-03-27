using DrugsApp.Features.TempDrugFeature.Shared;

namespace DrugsApp.Features.TempDrugFeature.ProcessTempDrugImportChunk;

internal static class ProcessTempDrugImportChunkMapping
{
    public static ProcessTempDrugImportChunkCommand MapToCommand(
        this List<ImportRowDto> rows,
        Guid batchId) => new(
        BatchId: batchId,
        Rows: rows);
}

