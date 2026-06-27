using System;

namespace DrugsApp.Features.AllergiesFeature.CheckOtherAllergyNameAvailability;

/// <summary>
/// Mapping helpers for other-allergy name availability checks.
/// </summary>
public static class CheckOtherAllergyNameAvailabilityMapping
{
    /// <summary>
    /// Maps the incoming request to a query envelope for processing.
    /// </summary>
    /// <param name="request">Other allergy name availability request.</param>
    /// <param name="userId">Identifier of the requesting user.</param>
    /// <returns>Query record for MediatR handling.</returns>
    public static CheckOtherAllergyNameAvailabilityQuery MapToQuery(
        this CheckOtherAllergyNameAvailabilityRequest request, Guid userId) =>
        new(userId, request.AllergyName, request.ExcludeId);
}
