using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.UpdateDrugAllergyNote;

/// <summary>
/// Mapping helpers for updating drug allergy notes.
/// </summary>
public static class UpdateDrugAllergyNoteMapping
{
    public static UpdateDrugAllergyNoteCommand MapToCommand(
        this UpdateAllergyNoteRequest request,
        Guid userId,
        Guid id) => new(userId, id, request);

    public static UpdatedDrugAllergyResponse MapToResponse(this ClientDrugAllergyDto item) =>
        new(item);
}
