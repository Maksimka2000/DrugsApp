using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.CreateOtherAllergy;

/// <summary>
/// Mapping helpers for creating other allergy records.
/// </summary>
public static class CreateOtherAllergyMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Create other allergy request payload.</param>
    /// <param name="userId">Identifier of the owning user.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static CreateOtherAllergyCommand MapToCommand(this CreateOtherAllergyRequest request, Guid userId) =>
        new(userId, request);

    /// <summary>
    /// Maps a created DTO to its API response.
    /// </summary>
    /// <param name="item">Created other allergy DTO.</param>
    /// <returns>Response wrapper.</returns>
    public static CreatedOtherAllergyResponse MapToResponse(this ClientOtherAllergyDto item) => new(item);
}
