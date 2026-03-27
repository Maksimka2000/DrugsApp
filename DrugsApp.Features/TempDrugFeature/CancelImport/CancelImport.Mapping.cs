using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Features.TempDrugFeature.CancelImport;

internal static class CancelImportMapping
{
    public static CancelImportCommand MapToCommand(Guid batchId) => new(BatchId: batchId);

    public static CancelImportResponse MapToResponse(this TempDrugImportBatch batch) => new(
        BatchId: batch.Id,
        Status: batch.StatusOfImport.ToString());
}

