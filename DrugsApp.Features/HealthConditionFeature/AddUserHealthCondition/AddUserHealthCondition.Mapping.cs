using System;

namespace DrugsApp.Features.HealthConditionFeature.AddUserHealthCondition;

/// <summary>
/// Mapping helpers for adding user health conditions.
/// </summary>
public static class AddUserHealthConditionMapping
{
    public static AddUserHealthConditionCommand MapToCommand(
        this AddUserHealthConditionRequest request,
        Guid userId) => new(userId, request);
}
