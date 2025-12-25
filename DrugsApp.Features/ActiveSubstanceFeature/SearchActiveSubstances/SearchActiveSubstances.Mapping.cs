using DrugsApp.Features.ActiveSubstanceFeature.Shared;

namespace DrugsApp.Features.ActiveSubstanceFeature.SearchActiveSubstances;

/// <summary>
/// Mapping extensions for SearchActiveSubstances use case.
/// </summary>
public static class SearchActiveSubstancesMapping
{
    /// <summary>
    /// Maps query parameters to SearchActiveSubstancesQuery.
    /// </summary>
    public static SearchActiveSubstancesQuery MapToQuery(this SearchActiveSubstancesRequest request) => new(
        Name: request.Name,
        Skip: request.Skip,
        Take: request.Take);

    /// <summary>
    /// Maps the search results envelope.
    /// </summary>
    public static SearchActiveSubstancesResponse MapToResponse(
        this List<ActiveSubstanceDto> items,
        int? totalCount,
        bool hasMore) => new(
        Items: items,
        TotalCount: totalCount,
        HasMore: hasMore);
}


