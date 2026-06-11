using System.Collections.Generic;
using DrugsApp.Features.DrugFeature.Shared;

namespace DrugsApp.Features.DrugFeature.SearchDrugForms;

internal static class SearchDrugFormsMapping
{
    public static SearchDrugFormsQuery MapToQuery(this SearchDrugFormsRequest request) => new(
        SearchQuery: request.SearchQuery,
        SortBy: request.SortBy,
        OrderBy: request.OrderBy,
        Skip: request.Skip,
        Take: request.Take);

    public static SearchDrugFormsResponse MapToResponse(
        this List<SearchDrugFormDto> items,
        int? totalCount,
        bool hasMore,
        SearchDrugFormsMetadata metadata) => new(
        Items: items,
        TotalCount: totalCount,
        HasMore: hasMore,
        Metadata: metadata);
}
