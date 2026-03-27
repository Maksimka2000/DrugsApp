namespace DrugsApp.Features.TempDrugFeature.CompleteTempDrugImport;

internal static class CompleteTempDrugImportMapping
{
    public static CompleteTempDrugImportCommand MapToCommand(Guid batchId) => new(BatchId: batchId);
}

