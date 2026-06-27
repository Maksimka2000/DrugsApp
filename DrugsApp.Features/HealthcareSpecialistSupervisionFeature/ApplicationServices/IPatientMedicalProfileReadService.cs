using DrugsApp.Features.DrugInteractionToolFeature.Shared;
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
/// Aggregated medical profile data for a patient.
/// </summary>
/// <param name="ReproductiveHealth">Reproductive health details.</param>
/// <param name="HealthConditions">Health conditions group.</param>
/// <param name="ProblematicDrugs">Problematic drugs group.</param>
/// <param name="Allergies">Allergies group.</param>
/// <param name="RegularDrugs">Current regular drugs.</param>
/// <param name="RegularFoods">Current regular foods.</param>
/// <param name="RegularSupplements">Current regular supplements.</param>
public sealed record ProviderPatientMedicalDetailData(
    ProviderReproductiveHealthDetailDto ReproductiveHealth,
    HealthConditionsGroupReadModel HealthConditions,
    ProblematicDrugsGroupReadModel ProblematicDrugs,
    AllergiesGroupReadModel Allergies,
    IReadOnlyCollection<RegularDrugListItemDto> RegularDrugs,
    IReadOnlyCollection<RegularFoodListItemDto> RegularFoods,
    IReadOnlyCollection<RegularSupplementListItemDto> RegularSupplements);
