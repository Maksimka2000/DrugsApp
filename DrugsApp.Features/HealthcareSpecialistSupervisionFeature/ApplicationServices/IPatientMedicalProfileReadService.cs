using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using DrugsApp.Features.RegularFoodFeature.Shared;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ApplicationServices;

public interface IPatientMedicalProfileReadService
{
    Task<ProviderPatientMedicalDetailData> LoadAsync(
        Guid patientUserId,
        CancellationToken cancellationToken);
}

public sealed record ProviderPatientMedicalDetailData(
    ProviderReproductiveHealthDetailDto ReproductiveHealth,
    HealthConditionsGroupReadModel HealthConditions,
    ProblematicDrugsGroupReadModel ProblematicDrugs,
    AllergiesGroupReadModel Allergies,
    IReadOnlyCollection<RegularDrugListItemDto> RegularDrugs,
    IReadOnlyCollection<RegularFoodListItemDto> RegularFoods,
    IReadOnlyCollection<RegularSupplementListItemDto> RegularSupplements);
