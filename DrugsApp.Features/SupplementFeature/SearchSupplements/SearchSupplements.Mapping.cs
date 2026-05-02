using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;
namespace DrugsApp.Features.SupplementFeature.SearchSupplements;

/// <summary>
/// Mapping helpers for SearchSupplements use case.
/// </summary>
public static class SearchSupplementsMapping
{
    public static SearchSupplementsQuery MapToQuery(this SearchSupplementsRequest request) => new(
        SupplementName: request.SupplementName,
        Skip: request.Skip,
        Take: request.Take);

    public static SearchSupplementsResponse MapToResponse(this List<Supplement> supplements, int? totalCount, bool hasMore) => new(
        Items: supplements.MapToDtoList(),
        TotalCount: totalCount,
        HasMore: hasMore);
}
