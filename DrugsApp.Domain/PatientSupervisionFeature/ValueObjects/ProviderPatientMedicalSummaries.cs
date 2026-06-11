namespace DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;

/// <summary>
/// Access-scoped medical summaries for provider patient detail.
/// </summary>
public sealed record ProviderPatientMedicalSummaries(
    object? WomenSpecificSummary,
    object? RegularDrugsSummary,
    object? RegularSupplementsSummary,
    object? RegularFoodSummary,
    object? AllergiesSummary,
    object? HealthConditionsSummary,
    object? DrugProblemsSummary);
