using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.UpdateSupplementAllergyNote;

public static class UpdateSupplementAllergyNoteMapping
{
    public static UpdateSupplementAllergyNoteCommand MapToCommand(this UpdateAllergyNoteRequest request, Guid userId, Guid id) =>
        new(userId, id, request);

    public static UpdatedSupplementAllergyResponse MapToResponse(this ClientSupplementAllergyDto item) => new(item);
}
