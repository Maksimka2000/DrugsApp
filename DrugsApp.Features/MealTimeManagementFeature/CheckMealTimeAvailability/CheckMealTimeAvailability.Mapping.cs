using System;

namespace DrugsApp.Features.MealTimeManagementFeature.CheckMealTimeAvailability;

public static class CheckMealTimeAvailabilityMapping
{
    public static CheckMealTimeAvailabilityQuery MapToQuery(
        this CheckMealTimeAvailabilityRequest request,
        Guid userId) =>
        new(
            UserId: userId,
            MealName: string.IsNullOrWhiteSpace(request.MealName) ? null : request.MealName,
            IntakeTime: request.IntakeTime,
            ExcludeId: request.ExcludeId);
}
