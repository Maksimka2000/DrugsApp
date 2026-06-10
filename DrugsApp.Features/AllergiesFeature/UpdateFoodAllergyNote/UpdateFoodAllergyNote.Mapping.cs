using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.UpdateFoodAllergyNote;

public static class UpdateFoodAllergyNoteMapping
{
    public static UpdateFoodAllergyNoteCommand MapToCommand(this UpdateAllergyNoteRequest request, Guid userId, Guid id) =>
        new(userId, id, request);

    public static UpdatedFoodAllergyResponse MapToResponse(this ClientFoodAllergyDto item) => new(item);
}
