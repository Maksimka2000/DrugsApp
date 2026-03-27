namespace DrugsApp.Features.TempDrugFeature.GetImportBatch;

internal static class GetImportBatchMapping
{
    public static GetImportBatchQuery MapToQuery(Guid id) => new(Id: id);
}

