using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Features.RegularFoodFeature.Shared;

namespace DrugsApp.Features.RegularFoodFeature.ListRegularFood;

/// <summary>
/// Mapping helpers for list regular food use case.
/// </summary>
internal static class ListRegularFoodMapping
{
    public static ListRegularFoodQuery MapToQuery(this ListRegularFoodRequest request, Guid userId) =>
        new(
            userId,
            request.FoodNameQuery,
            request.StartDate,
            request.Skip,
            request.Take);

    public static ListRegularFoodResponse MapToResponse(
        this IEnumerable<Domain.RegularFoodFeature.Entities.ClientRegularFood> items,
        int? totalCount,
        bool hasMore) =>
        new(
            items.Select(item => item.MapToListItemDto()).ToList(),
            totalCount,
            hasMore);
}
