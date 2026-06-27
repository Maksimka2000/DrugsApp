using System;

namespace DrugsApp.Features.MealTimeManagementFeature.CreateMealTime;

/// <summary>
/// Mapping helpers for creating meal times.
/// </summary>
public static class CreateMealTimeMapping
{
    /// <summary>
    /// Builds a create command for a new meal time.
    /// </summary>
    /// <param name="request">Meal time creation payload.</param>
    /// <param name="userId">Owner user identifier.</param>
    /// <returns>Create command.</returns>
    public static CreateMealTimeCommand MapToCommand(this CreateMealTimeRequest request, Guid userId) =>
        new(userId, request);
}
