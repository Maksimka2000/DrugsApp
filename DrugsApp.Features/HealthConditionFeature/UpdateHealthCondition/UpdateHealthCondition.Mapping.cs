using System;

namespace DrugsApp.Features.HealthConditionFeature.UpdateHealthCondition;

/// <summary>
/// Mapping helpers for update health condition use case.
/// </summary>
public static class UpdateHealthConditionMapping
{
    /// <summary>
    /// Builds a command to update a catalog health condition.
    /// </summary>
    /// <param name="request">Update payload.</param>
    /// <param name="id">Health condition identifier.</param>
    /// <returns>Update command.</returns>
    public static UpdateHealthConditionCommand MapToCommand(
        this UpdateHealthConditionRequest request,
        Guid id) => new(id, request);
}
