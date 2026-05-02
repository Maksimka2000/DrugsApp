namespace DrugsApp.Features.TempDrugFeature.GetImportBatchDetails;

internal static class GetImportBatchDetailsMapping
{
    public static GetImportBatchDetailsQuery MapToQuery(Guid id) => new(id);
}
