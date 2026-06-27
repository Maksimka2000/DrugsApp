using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using DrugsApp.Features.MealTimeManagementFeature.Shared;

namespace DrugsApp.Features.MealTimeManagementFeature.ListMealTimes;

/// <summary>
/// Mapping helpers for listing meal times.
/// </summary>
public static class ListMealTimesMapping
{
    /// <summary>
    /// Maps meal time entities to a list response.
    /// </summary>
    /// <param name="entities">Meal time entities.</param>
    /// <returns>Response with list items.</returns>
    public static ListMealTimesResponse MapToResponse(this List<ClientMealTime> entities) =>
        new(entities.MapToListItemDtoList());

    /// <summary>
    /// Maps meal time entities to a list response.
    /// </summary>
    /// <param name="entities">Meal time entities.</param>
    /// <returns>Response with list items.</returns>
    public static ListMealTimesResponse MapToResponse(this IEnumerable<ClientMealTime> entities) =>
        new(entities.MapToListItemDtoList());
}
