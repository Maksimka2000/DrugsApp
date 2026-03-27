using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Features.TempDrugFeature.RevertImport;

internal static class RevertImportMapping
{
    public static RevertImportCommand MapToCommand(Guid batchId) => new(BatchId: batchId);

    public static RevertImportResponse MapToResponse(this TempDrugImportBatch batch) => new(
        BatchId: batch.Id,
        Status: batch.StatusOfImport.ToString());
}

