using System.Collections.Generic;
using DrugsApp.Features.ActiveSubstanceGroupFeature.Shared;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.SearchActiveSubstanceGroups;

/// <summary>
/// Mapping helpers for search active substance groups use case.
/// </summary>
public static class SearchActiveSubstanceGroupsMapping
{
    /// <summary>
    /// Maps request to query.
    /// </summary>
    public static SearchActiveSubstanceGroupsQuery MapToQuery(this SearchActiveSubstanceGroupsRequest request) => new(
        ActiveSubstanceName: request.ActiveSubstanceName,
        Skip: request.Skip,
        Take: request.Take);

    /// <summary>
    /// Maps collection of DTOs to response envelope.
    /// </summary>
    public static SearchActiveSubstanceGroupsResponse MapToResponse(
        this List<ActiveSubstanceGroupDto> items,
        int? totalCount,
        bool hasMore) => new(
        Items: items,
        TotalCount: totalCount,
        HasMore: hasMore);
}
