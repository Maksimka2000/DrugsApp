using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.CreateFoodAllergy;

public static class CreateFoodAllergyMapping
{
    public static CreateFoodAllergyCommand MapToCommand(this CreateFoodAllergyRequest request, Guid userId) =>
        new(userId, request);

    public static CreatedFoodAllergyResponse MapToResponse(this ClientFoodAllergyDto item) => new(item);
}
