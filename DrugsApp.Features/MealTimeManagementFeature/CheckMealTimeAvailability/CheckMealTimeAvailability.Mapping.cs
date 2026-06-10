using System;
using DrugsApp.Features.MealTimeManagementFeature.Shared;

namespace DrugsApp.Features.MealTimeManagementFeature.CheckMealTimeAvailability;

public static class CheckMealTimeAvailabilityMapping
{
    public static CheckMealTimeAvailabilityQuery MapToQuery(
        this CheckMealTimeAvailabilityRequest request,
        Guid userId)
    {
        TimeOnly? intakeTime = null;
        if (!string.IsNullOrWhiteSpace(request.IntakeTime) &&
            MealTimeMappings.TryParseIntakeTime(request.IntakeTime, out TimeOnly parsedIntakeTime))
        {
            intakeTime = parsedIntakeTime;
        }

        return new CheckMealTimeAvailabilityQuery(
            UserId: userId,
            MealName: string.IsNullOrWhiteSpace(request.MealName) ? null : request.MealName,
            IntakeTime: intakeTime,
            ExcludeId: request.ExcludeId);
    }
}
