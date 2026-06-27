using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using DrugsApp.Features.RegularFoodFeature.Shared;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ApplicationServices;

/// <summary>
/// Builds aggregated patient medical profiles for healthcare specialists.
/// </summary>
public sealed class PatientMedicalProfileReadService(
    IClientReproductiveHealthProfileRepository reproductiveHealthProfileRepository,
    IClientRegularDrugRepository clientRegularDrugRepository,
    IClientRegularSupplementRepository clientRegularSupplementRepository,
    IClientRegularFoodRepository clientRegularFoodRepository,
    IClientAllergySummaryRepository clientAllergySummaryRepository,
    IClientDrugAllergyRepository clientDrugAllergyRepository,
    IClientSupplementAllergyRepository clientSupplementAllergyRepository,
    IClientFoodAllergyRepository clientFoodAllergyRepository,
    IClientOtherAllergyRepository clientOtherAllergyRepository,
    IUserHealthConditionRepository userHealthConditionRepository,
    IDrugProblemRepository drugProblemRepository) : IPatientMedicalProfileReadService
{
    private const int PageSize = 100;

    /// <summary>
    /// Loads a patient's medical profile including reproductive health, conditions, problems, allergies, and regular intake data.
    /// </summary>
    /// <param name="patientUserId">Identifier of the patient user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Composed medical detail data.</returns>
    public async Task<ProviderPatientMedicalDetailData> LoadAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        ProviderReproductiveHealthDetailDto reproductiveHealth =
            await LoadReproductiveHealthAsync(patientUserId, cancellationToken);
        List<UserHealthCondition> healthConditions = await LoadAllPagesAsync(
            (skip, take, token) => userHealthConditionRepository.SearchAsync(patientUserId, null, skip, take, token),
            cancellationToken);
        List<DrugProblem> drugProblems = await LoadAllPagesAsync(
            (skip, take, token) => drugProblemRepository.SearchByUserIdAsync(patientUserId, skip, take, token),
            cancellationToken);
        List<ClientDrugAllergy> drugAllergies = await LoadAllPagesAsync(
            (skip, take, token) => clientDrugAllergyRepository.SearchAsync(patientUserId, skip, take, token),
            cancellationToken);
        List<ClientFoodAllergy> foodAllergies = await LoadAllPagesAsync(
            (skip, take, token) => clientFoodAllergyRepository.SearchAsync(patientUserId, skip, take, token),
            cancellationToken);
        List<ClientSupplementAllergy> supplementAllergies = await LoadAllPagesAsync(
            (skip, take, token) => clientSupplementAllergyRepository.SearchAsync(patientUserId, skip, take, token),
            cancellationToken);
        List<ClientOtherAllergy> otherAllergies = await LoadAllPagesAsync(
            (skip, take, token) => clientOtherAllergyRepository.SearchAsync(patientUserId, skip, take, token),
            cancellationToken);
        ClientAllergySummary? allergySummary =
            await clientAllergySummaryRepository.GetByUserIdAsync(patientUserId, cancellationToken);
        List<ClientRegularDrug> regularDrugs = await LoadAllPagesAsync(
            (skip, take, token) => clientRegularDrugRepository.SearchLiveByUserIdAsync(
                patientUserId,
                drugNameQuery: null,
                startDate: null,
                skip,
                take,
                token),
            cancellationToken);
        List<ClientRegularFood> regularFoods = await LoadAllPagesAsync(
            (skip, take, token) => clientRegularFoodRepository.SearchLiveByUserIdAsync(
                patientUserId,
                foodNameQuery: null,
                startDate: null,
                skip,
                take,
                token),
            cancellationToken);
        List<ClientRegularSupplement> regularSupplements = await LoadAllPagesAsync(
            (skip, take, token) => clientRegularSupplementRepository.SearchLiveByUserIdAsync(
                patientUserId,
                nameQuery: null,
                startDate: null,
                skip,
                take,
                token),
            cancellationToken);

        var takenFoodIds = regularFoods.Select(row => row.FoodId).ToHashSet();
        var takenSupplementIds = regularSupplements
            .Where(row => row.Source == RegularSupplementSource.Database && row.SupplementId.HasValue)
            .Select(row => row.SupplementId!.Value)
            .ToHashSet();

        return new ProviderPatientMedicalDetailData(
            ReproductiveHealth: reproductiveHealth,
            HealthConditions: PatientMedicalProfileSectionBuilders.BuildHealthConditionsGroup(healthConditions),
            ProblematicDrugs: PatientMedicalProfileSectionBuilders.BuildProblematicDrugsGroup(drugProblems),
            Allergies: PatientMedicalProfileSectionBuilders.BuildAllergiesGroup(
                drugAllergies,
                foodAllergies,
                supplementAllergies,
                otherAllergies,
                allergySummary,
                takenFoodIds,
                takenSupplementIds),
            RegularDrugs: regularDrugs.Select(RegularDrugMappings.MapToListItemDto).ToList(),
            RegularFoods: regularFoods.Select(RegularFoodMappings.MapToListItemDto).ToList(),
            RegularSupplements: regularSupplements.Select(RegularSupplementMappings.MapToListItemDto).ToList());
    }

    private async Task<ProviderReproductiveHealthDetailDto> LoadReproductiveHealthAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        ClientReproductiveHealthProfile? profile = await reproductiveHealthProfileRepository
            .GetTrackedByUserIdAsync(patientUserId, cancellationToken);

        if (profile is null)
        {
            return new ProviderReproductiveHealthDetailDto(
                HasProfile: false,
                CurrentlyPregnant: null,
                PlanningToGetPregnant: null,
                PregnancyPlanningUnknown: null,
                Menopause: null,
                ContraceptionMethodCode: null);
        }

        return new ProviderReproductiveHealthDetailDto(
            HasProfile: true,
            CurrentlyPregnant: profile.CurrentlyPregnant,
            PlanningToGetPregnant: profile.PlanningToGetPregnant,
            PregnancyPlanningUnknown: profile.PregnancyPlanningUnknown,
            Menopause: profile.Menopause,
            ContraceptionMethodCode: profile.ContraceptionMethodCode?.ToString());
    }

    private static async Task<List<T>> LoadAllPagesAsync<T>(
        Func<int, int, CancellationToken, Task<List<T>>> fetchPage,
        CancellationToken cancellationToken)
    {
        List<T> results = new();
        int skip = 0;

        while (true)
        {
            List<T> page = await fetchPage(skip, PageSize, cancellationToken);
            if (page.Count == 0)
            {
                break;
            }

            results.AddRange(page);
            if (page.Count < PageSize)
            {
                break;
            }

            skip += PageSize;
        }

        return results;
    }
}
