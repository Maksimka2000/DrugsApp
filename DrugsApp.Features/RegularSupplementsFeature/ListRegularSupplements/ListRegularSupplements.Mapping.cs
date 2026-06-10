using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplements;

/// <summary>
/// Mapping helpers for list regular supplements use case.
/// </summary>
internal static class ListRegularSupplementsMapping
{
    public static ListRegularSupplementsQuery MapToQuery(this ListRegularSupplementsRequest request, Guid userId) =>
        new(
            userId,
            request.NameQuery,
            request.StartDate,
            request.Skip,
            request.Take);

    public static ListRegularSupplementsResponse MapToResponse(
        this IEnumerable<Domain.RegularSupplementsFeature.Entities.ClientRegularSupplement> items,
        int? totalCount,
        bool hasMore) =>
        new(
            items.Select(item => item.MapToListItemDto()).ToList(),
            totalCount,
            hasMore);
}
