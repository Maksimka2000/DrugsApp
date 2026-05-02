using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;
namespace DrugsApp.Features.SupplementFeature.SearchSupplementGroups;

/// <summary>
/// Mapping helpers for SearchSupplementGroups use case.
/// </summary>
public static class SearchSupplementGroupsMapping
{
    public static SearchSupplementGroupsQuery MapToQuery(this SearchSupplementGroupsRequest request) => new(
        SupplementNameQuery: request.SupplementNameQuery,
        Skip: request.Skip,
        Take: request.Take);

    public static SearchSupplementGroupsResponse MapToResponse(this List<SupplementGroup> rows, int? totalCount, bool hasMore, string? matchedFieldToken, List<string> matchedFailedFieldTokens) => new(
        Items: rows.MapToDtoList(),
        TotalCount: totalCount,
        HasMore: hasMore,
        MatchedFieldToken: matchedFieldToken,
        MatchedFailedFieldTokens: matchedFailedFieldTokens);
}
