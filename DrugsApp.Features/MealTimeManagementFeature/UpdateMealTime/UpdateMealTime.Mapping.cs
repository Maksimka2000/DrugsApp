using System;

namespace DrugsApp.Features.MealTimeManagementFeature.UpdateMealTime;

public static class UpdateMealTimeMapping
{
    public static UpdateMealTimeCommand MapToCommand(
        this UpdateMealTimeRequest request,
        Guid userId,
        Guid mealTimeId) =>
        new(userId, mealTimeId, request);
}
