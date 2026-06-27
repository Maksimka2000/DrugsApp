using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.UpdateSupplementAllergyNote;

/// <summary>
/// Mapping helpers for updating supplement allergy notes.
/// </summary>
public static class UpdateSupplementAllergyNoteMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Update note request.</param>
    /// <param name="userId">Identifier of the owning user.</param>
    /// <param name="id">Allergy identifier.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static UpdateSupplementAllergyNoteCommand MapToCommand(this UpdateAllergyNoteRequest request, Guid userId, Guid id) =>
        new(userId, id, request);

    /// <summary>
    /// Maps a DTO to the API response after an update.
    /// </summary>
    /// <param name="item">Updated supplement allergy DTO.</param>
    /// <returns>Response wrapper.</returns>
    public static UpdatedSupplementAllergyResponse MapToResponse(this ClientSupplementAllergyDto item) => new(item);
}
