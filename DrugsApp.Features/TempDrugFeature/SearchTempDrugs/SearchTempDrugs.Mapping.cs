using DrugsApp.Features.TempDrugFeature.Shared;

namespace DrugsApp.Features.TempDrugFeature.SearchTempDrugs;

internal static class SearchTempDrugsMapping
{
    public static SearchTempDrugsQuery MapToQuery(this SearchTempDrugsRequest request) => new(
        CommercialName: request.CommercialName,
        Statuses: request.Statuses,
        TypeOfProducts: request.TypeOfProducts,
        SortBy: request.SortBy,
        OrderBy: request.OrderBy,
        Skip: request.Skip,
        Take: request.Take);

    public static SearchTempDrugsResponse MapToResponse(
        this List<TempDrugDto> items,
        int? totalCount,
        bool hasMore) => new(
        Items: items,
        TotalCount: totalCount,
        HasMore: hasMore);
}


