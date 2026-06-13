using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Domain.PatientSupervisionFeature.Interfaces;
using DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using DrugsApp.Features.HealthConditionFeature.Shared;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ApplicationServices;

/// <summary>
/// Read-only medical summaries for provider patient detail.
/// </summary>
public sealed class ProviderPatientMedicalSummaryReadService(
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
    IDrugProblemRepository drugProblemRepository) : IProviderPatientMedicalSummaryReadService
{
    private const int SummaryTake = 20;

    public async Task<ProviderPatientMedicalSummaries> LoadSummariesAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        Task<ProviderWomenSpecificSummaryDto> womenTask = LoadWomenSpecificSummaryAsync(patientUserId, cancellationToken);
        Task<ProviderRegularDrugsSummaryDto> drugsTask = LoadRegularDrugsSummaryAsync(patientUserId, cancellationToken);
        Task<ProviderRegularSupplementsSummaryDto> supplementsTask = LoadRegularSupplementsSummaryAsync(patientUserId, cancellationToken);
        Task<ProviderRegularFoodSummaryDto> foodTask = LoadRegularFoodSummaryAsync(patientUserId, cancellationToken);
        Task<ProviderAllergiesSummaryDto> allergiesTask = LoadAllergiesSummaryAsync(patientUserId, cancellationToken);
        Task<ProviderHealthConditionsSummaryDto> conditionsTask = LoadHealthConditionsSummaryAsync(patientUserId, cancellationToken);
        Task<ProviderDrugProblemsSummaryDto> problemsTask = LoadDrugProblemsSummaryAsync(patientUserId, cancellationToken);

        await Task.WhenAll(
            womenTask,
            drugsTask,
            supplementsTask,
            foodTask,
            allergiesTask,
            conditionsTask,
            problemsTask);

        return new ProviderPatientMedicalSummaries(
            WomenSpecificSummary: await womenTask,
            RegularDrugsSummary: await drugsTask,
            RegularSupplementsSummary: await supplementsTask,
            RegularFoodSummary: await foodTask,
            AllergiesSummary: await allergiesTask,
            HealthConditionsSummary: await conditionsTask,
            DrugProblemsSummary: await problemsTask);
    }

    private async Task<ProviderWomenSpecificSummaryDto> LoadWomenSpecificSummaryAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        ClientReproductiveHealthProfile? profile = await reproductiveHealthProfileRepository
            .GetTrackedByUserIdAsync(patientUserId, cancellationToken);

        if (profile is null)
        {
            return new ProviderWomenSpecificSummaryDto(
                HasProfile: false,
                CurrentlyPregnant: null,
                PlanningToGetPregnant: null,
                PregnancyPlanningUnknown: null,
                Menopause: null,
                ContraceptionMethodCode: null);
        }

        return new ProviderWomenSpecificSummaryDto(
            HasProfile: true,
            CurrentlyPregnant: profile.CurrentlyPregnant,
            PlanningToGetPregnant: profile.PlanningToGetPregnant,
            PregnancyPlanningUnknown: profile.PregnancyPlanningUnknown,
            Menopause: profile.Menopause,
            ContraceptionMethodCode: profile.ContraceptionMethodCode?.ToString());
    }

    private async Task<ProviderRegularDrugsSummaryDto> LoadRegularDrugsSummaryAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        List<ClientRegularDrug> page = await clientRegularDrugRepository.SearchLiveByUserIdAsync(
            patientUserId,
            drugNameQuery: null,
            startDate: null,
            skip: 0,
            take: SummaryTake,
            cancellationToken);

        int totalCount = await clientRegularDrugRepository.CountLiveByUserIdAsync(
            patientUserId,
            drugNameQuery: null,
            startDate: null,
            cancellationToken);

        ProviderRegularDrugSummaryItemDto[] items = page
            .Select(row => new ProviderRegularDrugSummaryItemDto(
                row.Id,
                row.Drug?.CommercialName ?? "Unknown drug",
                row.StartDate,
                row.EndDate))
            .ToArray();

        return new ProviderRegularDrugsSummaryDto(totalCount, items);
    }

    private async Task<ProviderRegularSupplementsSummaryDto> LoadRegularSupplementsSummaryAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        List<ClientRegularSupplement> page = await clientRegularSupplementRepository.SearchLiveByUserIdAsync(
            patientUserId,
            nameQuery: null,
            startDate: null,
            skip: 0,
            take: SummaryTake,
            cancellationToken);

        int totalCount = await clientRegularSupplementRepository.CountLiveByUserIdAsync(
            patientUserId,
            nameQuery: null,
            startDate: null,
            cancellationToken);

        ProviderRegularSupplementSummaryItemDto[] items = page
            .Select(row => new ProviderRegularSupplementSummaryItemDto(
                row.Id,
                RegularSupplementMappings.ResolveDisplayName(row),
                row.StartDate,
                row.EndDate))
            .ToArray();

        return new ProviderRegularSupplementsSummaryDto(totalCount, items);
    }

    private async Task<ProviderRegularFoodSummaryDto> LoadRegularFoodSummaryAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        List<ClientRegularFood> page = await clientRegularFoodRepository.SearchLiveByUserIdAsync(
            patientUserId,
            foodNameQuery: null,
            startDate: null,
            skip: 0,
            take: SummaryTake,
            cancellationToken);

        int totalCount = await clientRegularFoodRepository.CountLiveByUserIdAsync(
            patientUserId,
            foodNameQuery: null,
            startDate: null,
            cancellationToken);

        ProviderRegularFoodSummaryItemDto[] items = page
            .Select(row => new ProviderRegularFoodSummaryItemDto(
                row.Id,
                row.Food?.Name ?? "Unknown food",
                row.StartDate,
                row.EndDate))
            .ToArray();

        return new ProviderRegularFoodSummaryDto(totalCount, items);
    }

    private async Task<ProviderAllergiesSummaryDto> LoadAllergiesSummaryAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        ClientAllergySummary? summary = await clientAllergySummaryRepository.GetByUserIdAsync(patientUserId, cancellationToken);

        ProviderAllergyGeneralSummaryDto? generalSummary = summary is null
            ? null
            : new ProviderAllergyGeneralSummaryDto(
                summary.SevereReactionStatus?.ToString(),
                summary.CarriesAllergyMedication,
                summary.PollenDustMitesAllergyStatus?.ToString(),
                summary.InsectVenomAllergyStatus?.ToString(),
                summary.LatexAllergyStatus?.ToString());

        int drugCount = await clientDrugAllergyRepository.CountAsync(patientUserId, cancellationToken);
        int supplementCount = await clientSupplementAllergyRepository.CountAsync(patientUserId, cancellationToken);
        int foodCount = await clientFoodAllergyRepository.CountAsync(patientUserId, cancellationToken);
        int otherCount = await clientOtherAllergyRepository.CountAsync(patientUserId, cancellationToken);

        return new ProviderAllergiesSummaryDto(
            generalSummary,
            drugCount,
            supplementCount,
            foodCount,
            otherCount);
    }

    private async Task<ProviderHealthConditionsSummaryDto> LoadHealthConditionsSummaryAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        List<UserHealthCondition> page = await userHealthConditionRepository.SearchAsync(
            patientUserId,
            displayNameQuery: null,
            skip: 0,
            take: SummaryTake,
            cancellationToken);

        int totalCount = await userHealthConditionRepository.CountAsync(patientUserId, displayNameQuery: null, cancellationToken);

        ProviderHealthConditionSummaryItemDto[] items = page
            .Select(row =>
            {
                UserHealthConditionDto mapped = row.MapToDto();
                return new ProviderHealthConditionSummaryItemDto(mapped.Id, mapped.DisplayName, mapped.Note);
            })
            .ToArray();

        return new ProviderHealthConditionsSummaryDto(totalCount, items);
    }

    private async Task<ProviderDrugProblemsSummaryDto> LoadDrugProblemsSummaryAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        List<DrugProblem> page = await drugProblemRepository.SearchByUserIdAsync(
            patientUserId,
            skip: 0,
            take: SummaryTake,
            cancellationToken);

        int totalCount = await drugProblemRepository.CountByUserIdAsync(patientUserId, cancellationToken);

        ProviderDrugProblemSummaryItemDto[] items = page
            .Select(row =>
            {
                DrugProblemListItemDto mapped = row.MapToListItemDto();
                return new ProviderDrugProblemSummaryItemDto(mapped.Id, mapped.DisplayName, mapped.ProblemNote);
            })
            .ToArray();

        return new ProviderDrugProblemsSummaryDto(totalCount, items);
    }
}
