namespace DrugsApp.Features.TempDrugFeature.UploadImportViaFile;

internal static class UploadImportViaFileMapping
{
    public static UploadImportViaFileCommand MapToCommand(this UploadImportViaFileRequest request) => new(
        Request: request);
}

