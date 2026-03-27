using DrugsApp.Features.TempDrugFeature.Shared;

namespace DrugsApp.Features.TempDrugFeature.SearchImportBatches;

internal static class SearchImportBatchesMapping
{
    public static SearchImportBatchesQuery MapToQuery(this SearchImportBatchesRequest request) => new(
        SortBy: request.SortBy,
        OrderBy: request.OrderBy,
        Skip: request.Skip,
        Take: request.Take);

    public static SearchImportBatchesResponse MapToResponse(
        this List<ImportBatchDto> items,
        int? totalCount,
        bool hasMore) => new(
        Items: items,
        TotalCount: totalCount,
        HasMore: hasMore);
}

