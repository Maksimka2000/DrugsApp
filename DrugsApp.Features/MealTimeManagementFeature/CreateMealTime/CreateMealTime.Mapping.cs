using System;

namespace DrugsApp.Features.MealTimeManagementFeature.CreateMealTime;

public static class CreateMealTimeMapping
{
    public static CreateMealTimeCommand MapToCommand(this CreateMealTimeRequest request, Guid userId) =>
        new(userId, request);
}
