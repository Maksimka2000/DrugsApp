using System;

namespace DrugsApp.Features.HealthConditionFeature.UpdateHealthCondition;

/// <summary>
/// Mapping helpers for update health condition use case.
/// </summary>
public static class UpdateHealthConditionMapping
{
    public static UpdateHealthConditionCommand MapToCommand(
        this UpdateHealthConditionRequest request,
        Guid id) => new(id, request);
}
