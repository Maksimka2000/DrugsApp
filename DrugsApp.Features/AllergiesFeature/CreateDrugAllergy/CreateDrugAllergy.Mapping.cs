using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.CreateDrugAllergy;

/// <summary>
/// Mapping helpers for creating drug allergies.
/// </summary>
public static class CreateDrugAllergyMapping
{
    public static CreateDrugAllergyCommand MapToCommand(
        this CreateDrugAllergyRequest request,
        Guid userId) => new(userId, request);

    public static CreatedDrugAllergyResponse MapToResponse(this ClientDrugAllergyDto item) =>
        new(item);
}
