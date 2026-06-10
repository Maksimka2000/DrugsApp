using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.CreateOtherAllergy;

public static class CreateOtherAllergyMapping
{
    public static CreateOtherAllergyCommand MapToCommand(this CreateOtherAllergyRequest request, Guid userId) =>
        new(userId, request);

    public static CreatedOtherAllergyResponse MapToResponse(this ClientOtherAllergyDto item) => new(item);
}
