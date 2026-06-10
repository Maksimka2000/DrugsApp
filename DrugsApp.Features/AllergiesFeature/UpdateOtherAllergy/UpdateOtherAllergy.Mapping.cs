using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.UpdateOtherAllergy;

public static class UpdateOtherAllergyMapping
{
    public static UpdateOtherAllergyCommand MapToCommand(this UpdateOtherAllergyRequest request, Guid userId, Guid id) =>
        new(userId, id, request);

    public static UpdatedOtherAllergyResponse MapToResponse(this ClientOtherAllergyDto item) => new(item);
}
