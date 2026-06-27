using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.UpdateDrugAllergyNote;

/// <summary>
/// Mapping helpers for updating drug allergy notes.
/// </summary>
public static class UpdateDrugAllergyNoteMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Update note request.</param>
    /// <param name="userId">Identifier of the owning user.</param>
    /// <param name="id">Allergy identifier.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static UpdateDrugAllergyNoteCommand MapToCommand(
        this UpdateAllergyNoteRequest request,
        Guid userId,
        Guid id) => new(userId, id, request);

    /// <summary>
    /// Maps a DTO to the API response after an update.
    /// </summary>
    /// <param name="item">Updated drug allergy DTO.</param>
    /// <returns>Response wrapper.</returns>
    public static UpdatedDrugAllergyResponse MapToResponse(this ClientDrugAllergyDto item) =>
        new(item);
}
