using System;

namespace DrugsApp.Features.AllergiesFeature.CheckDrugCustomNameAvailability;

/// <summary>
/// Mapping helpers for drug custom name availability checks.
/// </summary>
public static class CheckDrugCustomNameAvailabilityMapping
{
    /// <summary>
    /// Maps a request DTO to its query representation for handler processing.
    /// </summary>
    /// <param name="request">Incoming HTTP request payload.</param>
    /// <param name="userId">Identifier of the requesting user.</param>
    /// <returns>Query record used by the MediatR handler.</returns>
    public static CheckDrugCustomNameAvailabilityQuery MapToQuery(
        this CheckDrugCustomNameAvailabilityRequest request,
        Guid userId) => new(userId, request.CustomDrugName);
}
