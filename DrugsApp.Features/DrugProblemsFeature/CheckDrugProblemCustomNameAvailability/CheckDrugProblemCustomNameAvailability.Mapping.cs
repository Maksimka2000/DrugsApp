using System;

namespace DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemCustomNameAvailability;

/// <summary>
/// Mapping helpers for custom name availability.
/// </summary>
public static class CheckDrugProblemCustomNameAvailabilityMapping
{
    /// <summary>
    /// Creates a query to verify if a custom drug problem name is available for the user.
    /// </summary>
    /// <param name="request">Request containing the custom name and optional exclusion id.</param>
    /// <param name="userId">Identifier of the user owning the drug problems.</param>
    /// <returns>Query populated with user id, name, and exclusion id.</returns>
    public static CheckDrugProblemCustomNameAvailabilityQuery MapToQuery(
        this CheckDrugProblemCustomNameAvailabilityRequest request,
        Guid userId) =>
        new(userId, request.CustomDrugName, request.ExcludeId);
}
