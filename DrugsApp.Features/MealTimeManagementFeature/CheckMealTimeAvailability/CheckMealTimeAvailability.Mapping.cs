using System;

namespace DrugsApp.Features.MealTimeManagementFeature.CheckMealTimeAvailability;

/// <summary>
/// Mapping helpers for meal time availability checks.
/// </summary>
public static class CheckMealTimeAvailabilityMapping
{
    /// <summary>
    /// Builds a query to check if a meal time is available for a user.
    /// </summary>
    /// <param name="request">Request containing meal name, intake time, and optional exclusion id.</param>
    /// <param name="userId">User identifier.</param>
    /// <returns>Availability query.</returns>
    public static CheckMealTimeAvailabilityQuery MapToQuery(
        this CheckMealTimeAvailabilityRequest request,
        Guid userId) =>
        new(
            UserId: userId,
            MealName: string.IsNullOrWhiteSpace(request.MealName) ? null : request.MealName,
            IntakeTime: request.IntakeTime,
            ExcludeId: request.ExcludeId);
}
