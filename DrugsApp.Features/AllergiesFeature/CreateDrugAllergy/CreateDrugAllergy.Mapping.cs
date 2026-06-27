using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.CreateDrugAllergy;

/// <summary>
/// Mapping helpers for creating drug allergies.
/// </summary>
public static class CreateDrugAllergyMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Create drug allergy request payload.</param>
    /// <param name="userId">Identifier of the owning user.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static CreateDrugAllergyCommand MapToCommand(
        this CreateDrugAllergyRequest request,
        Guid userId) => new(userId, request);

    /// <summary>
    /// Maps a created DTO to its API response.
    /// </summary>
    /// <param name="item">Created drug allergy DTO.</param>
    /// <returns>Response wrapper.</returns>
    public static CreatedDrugAllergyResponse MapToResponse(this ClientDrugAllergyDto item) =>
        new(item);
}
