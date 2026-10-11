using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Features.StudyModeFeature.ApplicationServices;
using DrugsApp.Features.StudyModeFeature.Shared;

namespace DrugsApp.Features.StudyModeFeature.GenerateStudyPatientRandomData;

/// <summary>
/// Pure mapping helpers of GenerateStudyPatientRandomData (no I/O, no business rules).
/// </summary>
public static class GenerateStudyPatientRandomDataMapping
{
    /// <summary>
    /// Gets the route token of every section, matching the medical-data proxy route suffixes.
    /// </summary>
    public static IReadOnlyDictionary<string, StudyMedicalSection> SectionsByToken { get; } =
        new Dictionary<string, StudyMedicalSection>(StringComparer.OrdinalIgnoreCase)
        {
            ["drug-allergies"] = StudyMedicalSection.DrugAllergies,
            ["supplement-allergies"] = StudyMedicalSection.SupplementAllergies,
            ["food-allergies"] = StudyMedicalSection.FoodAllergies,
            ["other-allergies"] = StudyMedicalSection.OtherAllergies,
            ["health-conditions"] = StudyMedicalSection.HealthConditions,
            ["drug-problems"] = StudyMedicalSection.DrugProblems,
            ["regular-drugs"] = StudyMedicalSection.RegularDrugs,
            ["regular-supplements"] = StudyMedicalSection.RegularSupplements,
            ["own-supplements"] = StudyMedicalSection.OwnSupplements,
            ["regular-food"] = StudyMedicalSection.RegularFood,
            ["meal-times"] = StudyMedicalSection.MealTimes,
        };

    /// <summary>
    /// Composes the validated request from the route section token and the body.
    /// </summary>
    public static GenerateStudyPatientRandomDataRequest MapToRequest(this GenerateStudyPatientRandomDataBody body, string section) => new(
        Section: section,
        Count: body.Count);

    /// <summary>
    /// Maps the validated request and route patient to the MediatR command.
    /// </summary>
    public static GenerateStudyPatientRandomDataCommand MapToCommand(this GenerateStudyPatientRandomDataRequest request, Guid patientUserId) => new(
        PatientUserId: patientUserId,
        Section: SectionsByToken[request.Section],
        Count: request.Count);

    /// <summary>
    /// Maps the generation result to the HTTP response contract.
    /// </summary>
    public static StudyRandomDataResponse MapToResponse(this StudyRandomDataResult result) => new(
        Section: SectionsByToken.First(pair => pair.Value == result.Section).Key,
        RequestedCount: result.RequestedCount,
        CreatedCount: result.CreatedCount,
        LimitReached: result.LimitReached);
}
