using System;

namespace DrugsApp.Features.AllergiesFeature.CheckFoodCustomNameAvailability;

/// <summary>
/// Mapping helpers for food custom-name availability checks.
/// </summary>
public static class CheckFoodCustomNameAvailabilityMapping
{
    /// <summary>
    /// Maps the incoming request to a query envelope for processing.
    /// </summary>
    /// <param name="request">Custom food name availability request.</param>
    /// <param name="userId">Identifier of the requesting user.</param>
    /// <returns>Query record for MediatR handling.</returns>
    public static CheckFoodCustomNameAvailabilityQuery MapToQuery(
        this CheckFoodCustomNameAvailabilityRequest request, Guid userId) =>
        new(userId, request.CustomFoodName);
}
