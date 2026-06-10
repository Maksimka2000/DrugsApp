namespace DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemTypeNameAvailability;

/// <summary>
/// Mapping helpers for drug problem type name availability.
/// </summary>
public static class CheckDrugProblemTypeNameAvailabilityMapping
{
    public static CheckDrugProblemTypeNameAvailabilityQuery MapToQuery(
        this CheckDrugProblemTypeNameAvailabilityRequest request) =>
        new(request.ProblemTypeName, request.ExcludeId);
}
