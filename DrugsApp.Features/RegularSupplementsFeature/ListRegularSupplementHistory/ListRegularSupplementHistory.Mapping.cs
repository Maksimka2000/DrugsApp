using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplementHistory;

/// <summary>
/// Mapping helpers for list regular supplement history use case.
/// </summary>
internal static class ListRegularSupplementHistoryMapping
{
    public static ListRegularSupplementHistoryQuery MapToQuery(
        this ListRegularSupplementHistoryRequest request,
        Guid userId) =>
        new(
            userId,
            request.NameQuery,
            request.StartDate,
            request.EndDate,
            request.Skip,
            request.Take);

    public static ListRegularSupplementHistoryResponse MapToResponse(
        this IEnumerable<Domain.RegularSupplementsFeature.Entities.ClientRegularSupplement> items,
        int? totalCount,
        bool hasMore) =>
        new(
            items.Select(item => item.MapToHistoryItemDto()).ToList(),
            totalCount,
            hasMore);
}
