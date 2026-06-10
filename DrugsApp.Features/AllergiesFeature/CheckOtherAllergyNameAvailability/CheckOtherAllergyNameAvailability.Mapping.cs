using System;

namespace DrugsApp.Features.AllergiesFeature.CheckOtherAllergyNameAvailability;

public static class CheckOtherAllergyNameAvailabilityMapping
{
    public static CheckOtherAllergyNameAvailabilityQuery MapToQuery(
        this CheckOtherAllergyNameAvailabilityRequest request, Guid userId) =>
        new(userId, request.AllergyName, request.ExcludeId);
}
