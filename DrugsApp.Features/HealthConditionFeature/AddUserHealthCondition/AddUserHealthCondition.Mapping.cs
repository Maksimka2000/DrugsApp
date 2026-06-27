using System;

namespace DrugsApp.Features.HealthConditionFeature.AddUserHealthCondition;

/// <summary>
/// Mapping helpers for adding user health conditions.
/// </summary>
public static class AddUserHealthConditionMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope scoped to a user.
    /// </summary>
    /// <param name="request">Add user health condition request payload.</param>
    /// <param name="userId">Identifier of the owning user.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static AddUserHealthConditionCommand MapToCommand(
        this AddUserHealthConditionRequest request,
        Guid userId) => new(userId, request);
}
