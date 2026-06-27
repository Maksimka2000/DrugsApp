using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.CreateSupplementAllergy;

/// <summary>
/// Mapping helpers for creating supplement allergies.
/// </summary>
public static class CreateSupplementAllergyMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Create supplement allergy request payload.</param>
    /// <param name="userId">Identifier of the owning user.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static CreateSupplementAllergyCommand MapToCommand(this CreateSupplementAllergyRequest request, Guid userId) =>
        new(userId, request);

    /// <summary>
    /// Maps a created DTO to its API response.
    /// </summary>
    /// <param name="item">Created supplement allergy DTO.</param>
    /// <returns>Response wrapper.</returns>
    public static CreatedSupplementAllergyResponse MapToResponse(this ClientSupplementAllergyDto item) => new(item);
}
