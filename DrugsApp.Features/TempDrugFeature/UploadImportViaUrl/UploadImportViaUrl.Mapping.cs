namespace DrugsApp.Features.TempDrugFeature.UploadImportViaUrl;

internal static class UploadImportViaUrlMapping
{
    public static UploadImportViaUrlCommand MapToCommand(this UploadImportViaUrlRequest request) => new(
        Request: request);
}

