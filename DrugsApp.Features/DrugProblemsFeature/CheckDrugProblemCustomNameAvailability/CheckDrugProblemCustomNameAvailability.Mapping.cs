using System;

namespace DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemCustomNameAvailability;

/// <summary>
/// Mapping helpers for custom name availability.
/// </summary>
public static class CheckDrugProblemCustomNameAvailabilityMapping
{
    public static CheckDrugProblemCustomNameAvailabilityQuery MapToQuery(
        this CheckDrugProblemCustomNameAvailabilityRequest request,
        Guid userId) =>
        new(userId, request.CustomDrugName, request.ExcludeId);
}
