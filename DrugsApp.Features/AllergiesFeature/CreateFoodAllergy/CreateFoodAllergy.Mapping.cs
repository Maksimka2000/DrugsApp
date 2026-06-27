using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.CreateFoodAllergy;

/// <summary>
/// Mapping helpers for creating food allergies.
/// </summary>
public static class CreateFoodAllergyMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Create food allergy request payload.</param>
    /// <param name="userId">Identifier of the owning user.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static CreateFoodAllergyCommand MapToCommand(this CreateFoodAllergyRequest request, Guid userId) =>
        new(userId, request);

    /// <summary>
    /// Maps a created DTO to its API response.
    /// </summary>
    /// <param name="item">Created food allergy DTO.</param>
    /// <returns>Response wrapper.</returns>
    public static CreatedFoodAllergyResponse MapToResponse(this ClientFoodAllergyDto item) => new(item);
}
