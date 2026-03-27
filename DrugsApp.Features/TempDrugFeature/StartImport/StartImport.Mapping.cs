using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Features.TempDrugFeature.StartImport;

internal static class StartImportMapping
{
    public static StartImportCommand MapToCommand(
        this StartImportRequest request,
        Guid batchId) => new(
        BatchId: batchId,
        Request: request);

    public static StartImportResponse MapToResponse(this TempDrugImportBatch batch) => new(
        BatchId: batch.Id,
        Status: batch.StatusOfImport.ToString());
}

