using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;
namespace DrugsApp.Features.SupplementFeature.SearchSupplementGroups;

/// <summary>
/// Mapping helpers for SearchSupplementGroups use case.
/// </summary>
public static class SearchSupplementGroupsMapping
{
    /// <summary>
    /// Maps a <see cref="SearchSupplementGroupsRequest"/> instance to a <see cref="SearchSupplementGroupsQuery"/> instance.
    /// </summary>
    /// <param name="request">The supplement group search request containing query parameters such as supplement name, skip, and take values.</param>
    /// <returns>A <see cref="SearchSupplementGroupsQuery"/> representing the mapped query needed for supplement group search operations.</returns>
    public static SearchSupplementGroupsQuery MapToQuery(this SearchSupplementGroupsRequest request) => new(
        SupplementNameQuery: request.SupplementNameQuery,
        Skip: request.Skip,
        Take: request.Take);

    /// <summary>
    /// Maps a list of <see cref="SupplementGroup"/> objects to a <see cref="SearchSupplementGroupsResponse"/> object.
    /// </summary>
    /// <param name="rows">The list of <see cref="SupplementGroup"/> entities to be mapped.</param>
    /// <param name="totalCount">The total count of items available. Can be null if not applicable.</param>
    /// <param name="hasMore"/>A boolean indicating whether there are
    /// <param name="matchedFieldToken"></param>
    /// <param name="matchedFailedFieldTokens"></param>
    public static SearchSupplementGroupsResponse MapToResponse(this List<SupplementGroup> rows, int? totalCount,
        bool hasMore, string? matchedFieldToken, List<string> matchedFailedFieldTokens) => new(
        Items: rows.MapToDtoList(),
        TotalCount: totalCount,
        HasMore: hasMore,
        MatchedFieldToken: matchedFieldToken,
        MatchedFailedFieldTokens: matchedFailedFieldTokens);
}
