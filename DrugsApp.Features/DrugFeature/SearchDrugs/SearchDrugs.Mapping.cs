using System.Collections.Generic;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugFeature.Shared;

namespace DrugsApp.Features.DrugFeature.SearchDrugs;

internal static class SearchDrugsMapping
{
    public static SearchDrugsQuery MapToQuery(this SearchDrugsRequest request) => new(
        SearchQuery: request.SearchQuery,
        TypeOfDrugs: request.TypeOfDrugs,
        SortBy: request.SortBy,
        OrderBy: request.OrderBy,
        Skip: request.Skip,
        Take: request.Take);

    public static SearchDrugsResponse MapToResponse(
        this List<DrugDto> items,
        int? totalCount,
        bool hasMore,
        SearchDrugsMetadata metadata) => new(
        Items: items,
        TotalCount: totalCount,
        HasMore: hasMore,
        Metadata: metadata);
}

