using System;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.UpdateOtherAllergy;

/// <summary>
/// Mapping helpers for updating "other" allergy records.
/// </summary>
public static class UpdateOtherAllergyMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Update other allergy request.</param>
    /// <param name="userId">Identifier of the owning user.</param>
    /// <param name="id">Allergy identifier.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static UpdateOtherAllergyCommand MapToCommand(this UpdateOtherAllergyRequest request, Guid userId, Guid id) =>
        new(userId, id, request);

    /// <summary>
    /// Maps a DTO to the API response after an update.
    /// </summary>
    /// <param name="item">Updated other allergy DTO.</param>
    /// <returns>Response wrapper.</returns>
    public static UpdatedOtherAllergyResponse MapToResponse(this ClientOtherAllergyDto item) => new(item);
}
