namespace DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemTypeNameAvailability;

/// <summary>
/// Mapping helpers for drug problem type name availability.
/// </summary>
public static class CheckDrugProblemTypeNameAvailabilityMapping
{
    /// <summary>
    /// Builds a query to verify if a drug problem type name is available.
    /// </summary>
    /// <param name="request">Request containing the name to check and optional exclusion id.</param>
    /// <returns>Query populated with the name and exclusion id.</returns>
    public static CheckDrugProblemTypeNameAvailabilityQuery MapToQuery(
        this CheckDrugProblemTypeNameAvailabilityRequest request) =>
        new(request.ProblemTypeName, request.ExcludeId);
}
