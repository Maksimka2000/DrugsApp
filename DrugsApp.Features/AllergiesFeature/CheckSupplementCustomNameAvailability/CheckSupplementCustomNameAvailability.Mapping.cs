using System;

namespace DrugsApp.Features.AllergiesFeature.CheckSupplementCustomNameAvailability;

public static class CheckSupplementCustomNameAvailabilityMapping
{
    public static CheckSupplementCustomNameAvailabilityQuery MapToQuery(
        this CheckSupplementCustomNameAvailabilityRequest request, Guid userId) =>
        new(userId, request.CustomSupplementName);
}
