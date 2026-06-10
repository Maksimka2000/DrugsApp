using System;

namespace DrugsApp.Features.AllergiesFeature.CheckDrugCustomNameAvailability;

/// <summary>
/// Mapping helpers for drug custom name availability checks.
/// </summary>
public static class CheckDrugCustomNameAvailabilityMapping
{
    public static CheckDrugCustomNameAvailabilityQuery MapToQuery(
        this CheckDrugCustomNameAvailabilityRequest request,
        Guid userId) => new(userId, request.CustomDrugName);
}
