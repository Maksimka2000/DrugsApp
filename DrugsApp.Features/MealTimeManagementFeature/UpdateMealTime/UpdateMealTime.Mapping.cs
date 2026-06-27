using System;

namespace DrugsApp.Features.MealTimeManagementFeature.UpdateMealTime;

/// <summary>
/// Mapping helpers for updating meal times.
/// </summary>
public static class UpdateMealTimeMapping
{
    /// <summary>
    /// Builds an update command for an existing meal time.
    /// </summary>
    /// <param name="request">Update payload.</param>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="mealTimeId">Meal time identifier.</param>
    /// <returns>Update command.</returns>
    public static UpdateMealTimeCommand MapToCommand(
        this UpdateMealTimeRequest request,
        Guid userId,
        Guid mealTimeId) =>
        new(userId, mealTimeId, request);
}
