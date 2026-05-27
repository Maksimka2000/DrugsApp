using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;
namespace DrugsApp.Features.SupplementFeature.SearchSupplements;

/// <summary>
/// Mapping helpers for SearchSupplements use case.
/// </summary>
public static class SearchSupplementsMapping
{
    /// <summary>
    /// Maps a <see cref="SearchSupplementsRequest"/> instance to a <see cref="SearchSupplementsQuery"/> object.
    /// </summary>
    /// <param name="request">The request object containing search parameters for supplements.</param>
    /// <returns>A query object containing the search parameters for supplements, including pagination details.</returns>
    public static SearchSupplementsQuery MapToQuery(this SearchSupplementsRequest request) => new(
        SupplementName: request.SupplementName,
        Skip: request.Skip,
        Take: request.Take);

    /// <summary>
    /// Maps a collection of supplements, total count, and a boolean value indicating more items
    /// into a response object for supplement search.
    /// </summary>
    /// <param name="supplements">The list of supplement entities to be mapped.</param>
    /// <param name="totalCount">The total count of supplements, if available.</param>
    /// <param name="hasMore">Indicates whether there are more supplements available beyond the current page.</param>
    /// <returns>
    /// A <see cref="SearchSupplementsResponse"/> containing the mapped supplement data, total count,
    /// and information about additional available items.
    /// </returns>
    public static SearchSupplementsResponse MapToResponse(this List<Supplement> supplements, int? totalCount,
        bool hasMore) => new(
        Items: supplements.MapToDtoList(),
        TotalCount: totalCount,
        HasMore: hasMore);
}
