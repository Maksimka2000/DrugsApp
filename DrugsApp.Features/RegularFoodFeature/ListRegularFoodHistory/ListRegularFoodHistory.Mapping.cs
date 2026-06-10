using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Features.RegularFoodFeature.Shared;

namespace DrugsApp.Features.RegularFoodFeature.ListRegularFoodHistory;

/// <summary>
/// Mapping helpers for list regular food history use case.
/// </summary>
internal static class ListRegularFoodHistoryMapping
{
    public static ListRegularFoodHistoryQuery MapToQuery(this ListRegularFoodHistoryRequest request, Guid userId) =>
        new(
            userId,
            request.FoodNameQuery,
            request.StartDate,
            request.EndDate,
            request.Skip,
            request.Take);

    public static ListRegularFoodHistoryResponse MapToResponse(
        this IEnumerable<Domain.RegularFoodFeature.Entities.ClientRegularFood> items,
        int? totalCount,
        bool hasMore) =>
        new(
            items.Select(item => item.MapToHistoryItemDto()).ToList(),
            totalCount,
            hasMore);
}
