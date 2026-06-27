using System;

namespace DrugsApp.Features.AllergiesFeature.CheckSupplementCustomNameAvailability;

/// <summary>
/// Mapping helpers for supplement custom-name availability checks.
/// </summary>
public static class CheckSupplementCustomNameAvailabilityMapping
{
    /// <summary>
    /// Maps the incoming request to a query envelope for processing.
    /// </summary>
    /// <param name="request">Custom supplement name availability request.</param>
    /// <param name="userId">Identifier of the requesting user.</param>
    /// <returns>Query record for MediatR handling.</returns>
    public static CheckSupplementCustomNameAvailabilityQuery MapToQuery(
        this CheckSupplementCustomNameAvailabilityRequest request, Guid userId) =>
        new(userId, request.CustomSupplementName);
}
