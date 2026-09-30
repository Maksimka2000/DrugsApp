using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using DrugsApp.Features.HealthConditionFeature.Shared;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using DrugsApp.Features.RegularFoodFeature.Shared;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ApplicationServices;

/// <summary>
/// Service for reading a patient's medical profile for healthcare specialists.
/// </summary>
public interface IPatientMedicalProfileReadService
{
    /// <summary>
    /// Loads the full medical profile for the specified patient.
    /// </summary>
    /// <param name="patientUserId">Identifier of the patient user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Aggregated patient medical detail data.</returns>
    Task<ProviderPatientMedicalDetailData> LoadAsync(
        Guid patientUserId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Aggregated medical profile data for a patient, in the same DTO shapes the member endpoints return.
/// </summary>
/// <param name="ReproductiveHealth">Reproductive health details.</param>
/// <param name="AllergySummary">Allergy summary answers.</param>
/// <param name="HealthConditions">Health conditions.</param>
/// <param name="DrugProblems">Drug problems.</param>
/// <param name="DrugAllergies">Drug allergies.</param>
/// <param name="SupplementAllergies">Supplement allergies.</param>
/// <param name="FoodAllergies">Food allergies.</param>
/// <param name="OtherAllergies">Other allergies.</param>
/// <param name="RegularDrugs">Current regular drugs.</param>
/// <param name="RegularFoods">Current regular foods.</param>
/// <param name="RegularSupplements">Current regular supplements.</param>
/// <param name="RegularDrugHistory">Latest archived regular drugs.</param>
/// <param name="RegularFoodHistory">Latest archived regular foods.</param>
/// <param name="RegularSupplementHistory">Latest archived regular supplements.</param>
/// <param name="MealTimes">Patient meal times.</param>
public sealed record ProviderPatientMedicalDetailData(
    ProviderReproductiveHealthDetailDto ReproductiveHealth,
    AllergySummaryResponse? AllergySummary,
    IReadOnlyCollection<UserHealthConditionDto> HealthConditions,
    IReadOnlyCollection<DrugProblemListItemDto> DrugProblems,
    IReadOnlyCollection<ClientDrugAllergyDto> DrugAllergies,
    IReadOnlyCollection<ClientSupplementAllergyDto> SupplementAllergies,
    IReadOnlyCollection<ClientFoodAllergyDto> FoodAllergies,
    IReadOnlyCollection<ClientOtherAllergyDto> OtherAllergies,
    IReadOnlyCollection<RegularDrugListItemDto> RegularDrugs,
    IReadOnlyCollection<RegularFoodListItemDto> RegularFoods,
    IReadOnlyCollection<RegularSupplementListItemDto> RegularSupplements,
    ProviderIntakeHistoryDto<RegularDrugHistoryItemDto> RegularDrugHistory,
    ProviderIntakeHistoryDto<RegularFoodHistoryListItemDto> RegularFoodHistory,
    ProviderIntakeHistoryDto<RegularSupplementHistoryItemDto> RegularSupplementHistory,
    IReadOnlyCollection<MealTimeListItemDto> MealTimes);
