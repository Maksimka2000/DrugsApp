namespace DrugsApp.Features.TempDrugFeature.DownloadImportFile;

internal static class DownloadImportFileMapping
{
    public static DownloadImportFileQuery MapToQuery(Guid batchId) => new(BatchId: batchId);
}

