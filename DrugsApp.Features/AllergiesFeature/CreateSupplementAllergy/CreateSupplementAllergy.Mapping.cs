using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.CreateSupplementAllergy;

public static class CreateSupplementAllergyMapping
{
    public static CreateSupplementAllergyCommand MapToCommand(this CreateSupplementAllergyRequest request, Guid userId) =>
        new(userId, request);

    public static CreatedSupplementAllergyResponse MapToResponse(this ClientSupplementAllergyDto item) => new(item);
}
