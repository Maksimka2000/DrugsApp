using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using DrugsApp.Features.MealTimeManagementFeature.Shared;

namespace DrugsApp.Features.MealTimeManagementFeature.ListMealTimes;

public static class ListMealTimesMapping
{
    public static ListMealTimesResponse MapToResponse(this List<ClientMealTime> entities) =>
        new(entities.MapToListItemDtoList());

    public static ListMealTimesResponse MapToResponse(this IEnumerable<ClientMealTime> entities) =>
        new(entities.MapToListItemDtoList());
}
