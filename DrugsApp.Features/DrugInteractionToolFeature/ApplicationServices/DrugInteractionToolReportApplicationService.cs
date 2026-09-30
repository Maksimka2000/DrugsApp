using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Enums;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using DrugsApp.Domain.MealTimeManagementFeature.Repositories;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using DrugsApp.Domain.RegularDrugsFeature.Shared;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Domain.SystemTextFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.DrugInteractionToolFeature.ApplicationServices;

/// <summary>
/// Orchestrates drug interaction tool report composition for public and registered audiences [description].
/// </summary>
public sealed class DrugInteractionToolReportApplicationService(
    IDrugRepository _drugRepository,
    IInteractionRepository _interactionRepository,
    IActiveSubstanceGroupRepository _activeSubstanceGroupRepository,
    IFoodRepository _foodRepository,
    IFoodGroupRepository _foodGroupRepository,
    ISupplementRepository _supplementRepository,
    ISupplementGroupRepository _supplementGroupRepository,
    ISystemTextRepository _systemTextRepository,
    IClientRegularDrugRepository _clientRegularDrugRepository,
    IClientRegularFoodRepository _clientRegularFoodRepository,
    IClientRegularSupplementRepository _clientRegularSupplementRepository,
    IClientMealTimeRepository _clientMealTimeRepository,
    IUserHealthConditionRepository _userHealthConditionRepository,
    IDrugProblemRepository _drugProblemRepository,
    IClientDrugAllergyRepository _clientDrugAllergyRepository,
    IClientFoodAllergyRepository _clientFoodAllergyRepository,
    IClientSupplementAllergyRepository _clientSupplementAllergyRepository,
    IClientOtherAllergyRepository _clientOtherAllergyRepository,
    IClientAllergySummaryRepository _clientAllergySummaryRepository,
    IClientReproductiveHealthProfileRepository _clientReproductiveHealthProfileRepository)
    : IDrugInteractionToolReportApplicationService
{
    private const int PageSize = 40;
    private const string AlcoholDefaultTextKey = "activeSubstance.alcohol.defaultText";
    private static readonly List<ActiveSubstanceAllergyOverlapRowReadModel> EmptyOverlapRows = new();

    private sealed record DrugFormSelection(
        Drug Drug,
        DrugForm Form,
        SelectedDrugSource? Source,
        RegularDrugIntakeReadModel? RegularIntake = null);

    private sealed record CrossDrugPairKey(Guid LeftFormId, Guid RightFormId);

    /// <summary>
    /// Aggregated user-owned medical data loaded once per report and reused across every downstream builder.
    /// </summary>
    private sealed record UserMedicalBundle(
        List<UserHealthCondition> HealthConditions,
        List<DrugProblem> DrugProblems,
        List<ClientDrugAllergy> DrugAllergies,
        List<ClientFoodAllergy> FoodAllergies,
        List<ClientSupplementAllergy> SupplementAllergies,
        List<ClientOtherAllergy> OtherAllergies,
        ClientAllergySummary? AllergySummary,
        ClientReproductiveHealthProfile? ReproductiveHealth);

    /// <summary>
    /// Precomputed per-drug-form and per-substance lookups so the per-drug loop performs zero linear scans.
    /// </summary>
    private sealed record AllergyIndexes(
        Dictionary<Guid, List<SpecificDrugProblemRowReadModel>> ProblemsByDrugFormId,
        Dictionary<Guid, SpecificDrugDirectAllergyRowReadModel> DirectAllergyByDrugFormId,
        Dictionary<Guid, List<ActiveSubstanceAllergyOverlapRowReadModel>> OverlapBySubstanceId);

    /// <summary>
    /// Single bulk expanded-interaction load shared by cross-drug, food, and supplement slicing.
    /// </summary>
    private sealed class BulkExpandedContext
    {
        public required List<InteractionExpanded> DrugDrugRows { get; init; }

        public required List<InteractionExpanded> FoodRows { get; init; }

        public required List<InteractionExpanded> SupplementRows { get; init; }

        public required Dictionary<Guid, Interaction> InteractionById { get; init; }

        public required Dictionary<Guid, Food> FoodById { get; init; }

        public required Dictionary<Guid, FoodGroup> FoodGroupById { get; init; }

        public required Dictionary<Guid, Supplement> SupplementById { get; init; }

        public required Dictionary<Guid, SupplementGroup> SupplementGroupById { get; init; }
    }

    /// <summary>
    /// Builds a public (anonymous) drug interaction tool report from manual selections.
    /// </summary>
    /// <param name="manualDrugFormIds">Manual drug form identifiers provided by the caller.</param>
    /// <param name="showAllDrugsInteractions">Whether to include all drug interactions.</param>
    /// <param name="showOralGuidance">Whether to include oral guidance sections.</param>
    /// <param name="showAllFoodAndSupplementInteractions">Whether to include all food/supplement interactions.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Report read model or error collection.</returns>
    public async Task<ErrorOr<PublicDrugInteractionToolReportReadModel>> ComputePublicReportAsync(
        IReadOnlyCollection<Guid> manualDrugFormIds,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        CancellationToken cancellationToken)
    {
        ErrorOr<IReadOnlyList<DrugFormSelection>> selectionsResult = await LoadManualSelectionsAsync(
            manualDrugFormIds,
            source: null,
            cancellationToken);

        if (selectionsResult.IsError)
        {
            return selectionsResult.Errors;
        }

        return await BuildPublicReportAsync(
            selectionsResult.Value,
            manualDrugFormIds.Count,
            showAllDrugsInteractions,
            showOralGuidance,
            showAllFoodAndSupplementInteractions,
            cancellationToken);
    }

    /// <summary>
    /// Builds a registered (authenticated) drug interaction tool report with optional user data.
    /// </summary>
    /// <param name="userId">User identifier owning the report context.</param>
    /// <param name="manualDrugFormIds">Manual drug form identifiers provided by the caller.</param>
    /// <param name="includeUserData">Whether to merge user data (regular intakes, allergies, problems).</param>
    /// <param name="showAllDrugsInteractions">Whether to include all drug interactions.</param>
    /// <param name="showOralGuidance">Whether to include oral guidance sections.</param>
    /// <param name="showAllFoodAndSupplementInteractions">Whether to include all food/supplement interactions.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <param name="additionalTakenNowFoodIds"></param>
    /// <param name="additionalTakenNowSupplementIds"></param>
    /// <returns>Registered report read model or error collection.</returns>
    public async Task<ErrorOr<RegisteredDrugInteractionToolReportReadModel>> ComputeRegisteredReportAsync(
        Guid userId,
        IReadOnlyCollection<Guid> manualDrugFormIds,
        bool includeUserData,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        CancellationToken cancellationToken,
        IReadOnlyCollection<Guid>? additionalTakenNowFoodIds = null,
        IReadOnlyCollection<Guid>? additionalTakenNowSupplementIds = null)
    {
        ErrorOr<IReadOnlyList<DrugFormSelection>> selectionsResult = await LoadManualSelectionsAsync(
            manualDrugFormIds,
            source: SelectedDrugSource.Manual,
            cancellationToken);

        if (selectionsResult.IsError)
        {
            return selectionsResult.Errors;
        }

        var effectiveSelections = selectionsResult.Value.ToList();

        UserInfoSectionReadModel? userInfoSection = null;
        HashSet<Guid> takenNowFoodIds = new();
        HashSet<Guid> takenNowSupplementIds = new();
        AllergyIndexes allergyIndexes = new(new(), new(), new());

        if (includeUserData)
        {
            List<ClientRegularDrug> regularDrugs = await LoadAllLiveRegularDrugsAsync(userId, cancellationToken);
            List<ClientRegularFood> regularFoods = await LoadAllLiveRegularFoodsAsync(userId, cancellationToken);
            List<ClientRegularSupplement> regularSupplements = await LoadAllLiveRegularSupplementsAsync(userId, cancellationToken);

            effectiveSelections = MergeRegularDrugs(effectiveSelections, regularDrugs);

            takenNowFoodIds = regularFoods.Select(regularFood => regularFood.FoodId).ToHashSet();
            takenNowSupplementIds = regularSupplements
                .Where(regularSupplement => regularSupplement.Source == RegularSupplementSource.Database
                    && regularSupplement.SupplementId.HasValue)
                .Select(regularSupplement => regularSupplement.SupplementId!.Value)
                .ToHashSet();

            if (additionalTakenNowFoodIds is { Count: > 0 })
            {
                foreach (Guid foodId in additionalTakenNowFoodIds)
                {
                    takenNowFoodIds.Add(foodId);
                }
            }

            if (additionalTakenNowSupplementIds is { Count: > 0 })
            {
                foreach (Guid supplementId in additionalTakenNowSupplementIds)
                {
                    takenNowSupplementIds.Add(supplementId);
                }
            }

            UserMedicalBundle bundle = await LoadUserMedicalBundleAsync(userId, cancellationToken);
            allergyIndexes = BuildAllergyIndexes(bundle.DrugAllergies, bundle.DrugProblems);
            userInfoSection = BuildUserInfoSection(
                bundle,
                regularDrugs,
                takenNowFoodIds,
                takenNowSupplementIds);
        }

        return await BuildRegisteredReportAsync(
            userId,
            effectiveSelections,
            manualDrugFormIds.Count,
            includeUserData,
            showAllDrugsInteractions,
            showOralGuidance,
            showAllFoodAndSupplementInteractions,
            userInfoSection,
            takenNowFoodIds,
            takenNowSupplementIds,
            allergyIndexes,
            cancellationToken);
    }

    private async Task<ErrorOr<IReadOnlyList<DrugFormSelection>>> LoadManualSelectionsAsync(
        IReadOnlyCollection<Guid> manualDrugFormIds,
        SelectedDrugSource? source,
        CancellationToken cancellationToken)
    {
        List<Drug> drugs = await _drugRepository.GetByDrugFormIdsAsync(manualDrugFormIds, cancellationToken);
        Dictionary<Guid, (Drug Drug, DrugForm Form)> formLookup = new();

        foreach (Drug drug in drugs)
        {
            foreach (DrugForm form in drug.DrugForms)
            {
                formLookup[form.Id] = (drug, form);
            }
        }

        List<DrugFormSelection> selections = new();
        foreach (Guid formId in manualDrugFormIds)
        {
            if (!formLookup.TryGetValue(formId, out (Drug Drug, DrugForm Form) match))
            {
                return Error.Validation(
                    code: "DrugInteractionTool.DrugFormNotFound",
                    description: "One or more selected drug forms were not found.",
                    metadata: new Dictionary<string, object>
                    {
                        ["missingResourceIds"] = formId.ToString()
                    });
            }

            ErrorOr<Success> eligibilityResult = ValidateDrugEligibility(match.Drug, formId);
            if (eligibilityResult.IsError)
            {
                return eligibilityResult.Errors;
            }

            selections.Add(new DrugFormSelection(match.Drug, match.Form, source));
        }

        return selections;
    }

    private static ErrorOr<Success> ValidateDrugEligibility(Drug drug, Guid formId)
    {
        if (RegularDrugEligibility.IsEligibleDrug(drug))
        {
            return Result.Success;
        }

        string code = drug.TypeOfDrug switch
        {
            DrugType.Special => "DrugInteractionTool.IneligibleDrugSpecial",
            DrugType.Ophthalmic => "DrugInteractionTool.IneligibleDrugOphthalmic",
            _ when drug.TypeOfProduct.Equals("Weterynaryjny", StringComparison.OrdinalIgnoreCase)
                => "DrugInteractionTool.IneligibleDrugVeterinary",
            _ => "DrugInteractionTool.IneligibleDrug"
        };

        return Error.Validation(
            code: code,
            description: "Selected drug is not eligible for interaction computation.",
            metadata: new Dictionary<string, object>
            {
                ["drugFormId"] = formId.ToString(),
                ["drugId"] = drug.Id.ToString()
            });
    }

    private static List<DrugFormSelection> MergeRegularDrugs(
        List<DrugFormSelection> manualSelections,
        List<ClientRegularDrug> regularRows)
    {
        var byFormId = manualSelections.ToDictionary(selection => selection.Form.Id);

        foreach (ClientRegularDrug regularRow in regularRows)
        {
            if (!RegularDrugEligibility.IsEligibleDrug(regularRow.Drug))
            {
                continue;
            }

            if (byFormId.TryGetValue(regularRow.DrugFormId, out DrugFormSelection? existing))
            {
                byFormId[regularRow.DrugFormId] = existing with
                {
                    Source = SelectedDrugSource.ManualAndUserRegular,
                    RegularIntake = DrugInteractionToolReportMapping.MapRegularIntakeSnapshot(regularRow)
                };
                continue;
            }

            byFormId[regularRow.DrugFormId] = new DrugFormSelection(
                regularRow.Drug,
                regularRow.DrugForm,
                SelectedDrugSource.UserRegular,
                DrugInteractionToolReportMapping.MapRegularIntakeSnapshot(regularRow));
        }

        return byFormId.Values
            .OrderBy(selection => selection.Drug.CommercialName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(selection => selection.Form.Id)
            .ToList();
    }

    private async Task<ErrorOr<PublicDrugInteractionToolReportReadModel>> BuildPublicReportAsync(
        IReadOnlyList<DrugFormSelection> selections,
        int manualDrugCount,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        CancellationToken cancellationToken)
    {
        ReportCompositionContext context = BuildCompositionContext(selections);

        BulkExpandedContext bulk = await BuildBulkExpandedContextAsync(
            context,
            includeFood: showAllFoodAndSupplementInteractions,
            includeSupplement: showAllFoodAndSupplementInteractions,
            cancellationToken);

        Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> crossDrugPairs =
            BuildCrossDrugPairs(context, bulk);

        DrugDuplicationSectionReadModel duplicationSection = await BuildDuplicationSectionAsync(context, cancellationToken);
        Dictionary<Guid, AlcoholInteractionRowReadModel> alcoholRows = await BuildAlcoholRowsAsync(context, cancellationToken);

        List<PublicSpecificDrugReportReadModel> specificReports = new();

        foreach (DrugFormSelection selection in selections)
        {
            SpecificDrugDetailReadModel drugDetail = context.DrugDetailsByFormId[selection.Form.Id];
            AlcoholInteractionSectionReadModel alcohol = SliceAlcoholSection(selection, alcoholRows);

            DrugInteractionSectionReadModel drugsSection = BuildPerDrugDrugSection(
                selection.Form.Id,
                context,
                crossDrugPairs);

            PublicFoodInteractionSectionReadModel? foods = null;
            PublicSupplementInteractionSectionReadModel? supplements = null;

            if (showAllFoodAndSupplementInteractions)
            {
                List<FoodInteractionRowReadModel> foodRows = SliceFoodRows(
                    selection,
                    context,
                    bulk,
                    takenNowFoodIds: null,
                    includeAllCatalog: true);

                foods = new PublicFoodInteractionSectionReadModel(
                    SectionCode: "specific_drug.foods",
                    AllOther: DrugInteractionToolReportMapping.BuildSubsection(
                        "specific_drug.foods.all_other",
                        DrugInteractionToolHeadlines.FoodAllOtherPublic,
                        foodRows));

                List<SupplementInteractionRowReadModel> supplementRows = SliceSupplementRows(
                    selection,
                    context,
                    bulk,
                    takenNowSupplementIds: null,
                    includeAllCatalog: true);

                supplements = new PublicSupplementInteractionSectionReadModel(
                    SectionCode: "specific_drug.supplements",
                    AllOther: DrugInteractionToolReportMapping.BuildSubsection(
                        "specific_drug.supplements.all_other",
                        DrugInteractionToolHeadlines.SupplementAllOtherPublic,
                        supplementRows));
            }

            specificReports.Add(new PublicSpecificDrugReportReadModel(
                Drug: drugDetail,
                Alcohol: alcohol,
                Drugs: drugsSection,
                Foods: foods,
                Supplements: supplements));
        }

        AllDrugsInteractionsSectionReadModel? globalSection = showAllDrugsInteractions
            ? BuildGlobalSection(selections, context, crossDrugPairs)
            : null;

        return new PublicDrugInteractionToolReportReadModel(
            SettingsSnapshot: new PublicDrugInteractionToolSettingsSnapshotReadModel(
                ShowAllDrugsInteractions: showAllDrugsInteractions,
                ShowOralGuidance: showOralGuidance,
                ShowAllFoodAndSupplementInteractions: showAllFoodAndSupplementInteractions,
                ManualDrugLimit: manualDrugCount),
            SelectedDrugs: selections.Select(selection => context.TargetSummariesByFormId[selection.Form.Id]).ToList(),
            SpecificDrugReports: specificReports,
            AllDrugsInteractionsSection: globalSection,
            DrugDuplicationSection: duplicationSection);
    }

    private async Task<ErrorOr<RegisteredDrugInteractionToolReportReadModel>> BuildRegisteredReportAsync(
        Guid userId,
        IReadOnlyList<DrugFormSelection> selections,
        int manualDrugCount,
        bool includeUserData,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        UserInfoSectionReadModel? userInfoSection,
        HashSet<Guid> takenNowFoodIds,
        HashSet<Guid> takenNowSupplementIds,
        AllergyIndexes allergyIndexes,
        CancellationToken cancellationToken)
    {
        ReportCompositionContext context = BuildCompositionContext(selections);

        bool includeFoodOrSupplementBulk = includeUserData || showAllFoodAndSupplementInteractions;

        BulkExpandedContext bulk = await BuildBulkExpandedContextAsync(
            context,
            includeFood: includeFoodOrSupplementBulk,
            includeSupplement: includeFoodOrSupplementBulk,
            cancellationToken);

        Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> crossDrugPairs =
            BuildCrossDrugPairs(context, bulk);

        DrugDuplicationSectionReadModel duplicationSection = await BuildDuplicationSectionAsync(context, cancellationToken);
        Dictionary<Guid, AlcoholInteractionRowReadModel> alcoholRows = await BuildAlcoholRowsAsync(context, cancellationToken);

        List<ClientMealTime> mealTimes = includeUserData
            ? await _clientMealTimeRepository.ListByUserAsync(userId, cancellationToken)
            : new List<ClientMealTime>();

        List<RegisteredSpecificDrugReportReadModel> specificReports = new();

        foreach (DrugFormSelection selection in selections)
        {
            SpecificDrugDetailReadModel drugDetail = context.DrugDetailsByFormId[selection.Form.Id];
            AlcoholInteractionSectionReadModel alcohol = SliceAlcoholSection(selection, alcoholRows);

            DrugInteractionSectionReadModel drugsSection = BuildPerDrugDrugSection(
                selection.Form.Id,
                context,
                crossDrugPairs);

            SpecificDrugProblemsSectionReadModel? problems = includeUserData
                ? SliceProblems(selection.Form.Id, allergyIndexes)
                : null;

            SpecificDrugAllergiesSectionReadModel? allergies = includeUserData
                ? SliceAllergies(selection, allergyIndexes)
                : null;

            RegisteredFoodInteractionSectionReadModel? foods = SliceRegisteredFoodSection(
                selection,
                context,
                bulk,
                includeUserData,
                showOralGuidance,
                showAllFoodAndSupplementInteractions,
                takenNowFoodIds,
                mealTimes);

            RegisteredSupplementInteractionSectionReadModel? supplements = SliceRegisteredSupplementSection(
                selection,
                context,
                bulk,
                includeUserData,
                showAllFoodAndSupplementInteractions,
                takenNowSupplementIds);

            specificReports.Add(new RegisteredSpecificDrugReportReadModel(
                Drug: drugDetail,
                Problems: problems,
                Allergies: allergies,
                Alcohol: alcohol,
                Drugs: drugsSection,
                Foods: foods,
                Supplements: supplements));
        }

        AllDrugsInteractionsSectionReadModel? globalSection = showAllDrugsInteractions
            ? BuildGlobalSection(selections, context, crossDrugPairs)
            : null;

        return new RegisteredDrugInteractionToolReportReadModel(
            SettingsSnapshot: new RegisteredDrugInteractionToolSettingsSnapshotReadModel(
                IncludeUserData: includeUserData,
                ShowAllDrugsInteractions: showAllDrugsInteractions,
                ShowOralGuidance: showOralGuidance,
                ShowAllFoodAndSupplementInteractions: showAllFoodAndSupplementInteractions,
                ManualDrugLimit: manualDrugCount),
            UserInfoSection: userInfoSection,
            SelectedDrugs: selections.Select(selection => context.TargetSummariesByFormId[selection.Form.Id]).ToList(),
            SpecificDrugReports: specificReports,
            AllDrugsInteractionsSection: globalSection,
            DrugDuplicationSection: duplicationSection);
    }

    private sealed class ReportCompositionContext
    {
        public required IReadOnlyList<DrugFormSelection> Selections { get; init; }

        public required Dictionary<Guid, DrugTargetSummaryReadModel> TargetSummariesByFormId { get; init; }

        public required Dictionary<Guid, SpecificDrugDetailReadModel> DrugDetailsByFormId { get; init; }

        public required Dictionary<Guid, ActiveSubstance> ActiveSubstanceById { get; init; }

        public required Dictionary<Guid, string> ActiveSubstanceNameById { get; init; }
    }

    private static ReportCompositionContext BuildCompositionContext(IReadOnlyList<DrugFormSelection> selections)
    {
        var activeSubstanceById = selections
            .SelectMany(selection => selection.Drug.ActiveSubstances)
            .GroupBy(substance => substance.Id)
            .ToDictionary(group => group.Key, group => group.First());

        Dictionary<Guid, DrugTargetSummaryReadModel> targetSummariesByFormId = new();
        Dictionary<Guid, SpecificDrugDetailReadModel> drugDetailsByFormId = new();

        foreach (DrugFormSelection selection in selections)
        {
            targetSummariesByFormId[selection.Form.Id] = DrugInteractionToolReportMapping.MapDrugTargetSummary(
                selection.Drug,
                selection.Form);
            drugDetailsByFormId[selection.Form.Id] = DrugInteractionToolReportMapping.MapSpecificDrugDetail(
                selection.Drug,
                selection.Form,
                selection.Source,
                selection.RegularIntake);
        }

        var substanceNameById = activeSubstanceById.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ActiveSubstanceName);

        return new ReportCompositionContext
        {
            Selections = selections,
            TargetSummariesByFormId = targetSummariesByFormId,
            DrugDetailsByFormId = drugDetailsByFormId,
            ActiveSubstanceById = activeSubstanceById,
            ActiveSubstanceNameById = substanceNameById
        };
    }

    private async Task<BulkExpandedContext> BuildBulkExpandedContextAsync(
        ReportCompositionContext context,
        bool includeFood,
        bool includeSupplement,
        CancellationToken cancellationToken)
    {
        var allSubstanceIds = context.ActiveSubstanceNameById.Keys.ToHashSet();

        List<InteractionExpanded> allRows = await _interactionRepository.SearchExpandedRowsBulkAsync(
            allSubstanceIds,
            includeFood,
            includeSupplement,
            cancellationToken);

        var drugDrugRows = allRows.Where(row => row.PartyTypeB == ShortInteractionType.Substance).ToList();
        var foodRows = allRows.Where(row => row.PartyTypeB == ShortInteractionType.Food).ToList();
        var supplementRows = allRows.Where(row => row.PartyTypeB == ShortInteractionType.Supplement).ToList();

        var interactionIds = allRows.Select(row => row.InteractionId).ToHashSet();
        List<Interaction> interactions = await _interactionRepository.GetByIdsAsync(interactionIds, cancellationToken);
        var interactionById = interactions.ToDictionary(interaction => interaction.Id);

        var foodIds = foodRows.Select(row => row.PartyIdB).Distinct().ToList();
        var foodById = foodIds.Count > 0
            ? (await _foodRepository.GetByIdsAsync(foodIds, cancellationToken)).ToDictionary(food => food.Id)
            : new Dictionary<Guid, Food>();

        var foodGroupIds = interactions
            .SelectMany(interaction => interaction.Parties)
            .Where(party => party.PartyType == InteractionType.FoodGroup && party.FoodGroupId.HasValue)
            .Select(party => party.FoodGroupId!.Value)
            .Distinct()
            .ToList();
        var foodGroupById = foodGroupIds.Count > 0
            ? (await _foodGroupRepository.GetByIdsAsync(foodGroupIds, cancellationToken)).ToDictionary(group => group.Id)
            : new Dictionary<Guid, FoodGroup>();

        var supplementIds = supplementRows.Select(row => row.PartyIdB).Distinct().ToList();
        var supplementById = supplementIds.Count > 0
            ? (await _supplementRepository.GetByIdsAsync(supplementIds, cancellationToken)).ToDictionary(supplement => supplement.Id)
            : new Dictionary<Guid, Supplement>();

        var supplementGroupIds = interactions
            .SelectMany(interaction => interaction.Parties)
            .Where(party => party.PartyType == InteractionType.SupplementGroup && party.SupplementGroupId.HasValue)
            .Select(party => party.SupplementGroupId!.Value)
            .Distinct()
            .ToList();
        var supplementGroupById = supplementGroupIds.Count > 0
            ? (await _supplementGroupRepository.GetByIdsAsync(supplementGroupIds, cancellationToken)).ToDictionary(group => group.Id)
            : new Dictionary<Guid, SupplementGroup>();

        return new BulkExpandedContext
        {
            DrugDrugRows = drugDrugRows,
            FoodRows = foodRows,
            SupplementRows = supplementRows,
            InteractionById = interactionById,
            FoodById = foodById,
            FoodGroupById = foodGroupById,
            SupplementById = supplementById,
            SupplementGroupById = supplementGroupById
        };
    }

    private static Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> BuildCrossDrugPairs(
        ReportCompositionContext context,
        BulkExpandedContext bulk)
    {
        Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> result = new();
        var selections = context.Selections.ToList();

        for (int leftIndex = 0; leftIndex < selections.Count; leftIndex++)
        {
            for (int rightIndex = leftIndex + 1; rightIndex < selections.Count; rightIndex++)
            {
                DrugFormSelection left = selections[leftIndex];
                DrugFormSelection right = selections[rightIndex];

                var leftIds = left.Drug.ActiveSubstances.Select(substance => substance.Id).ToHashSet();
                var rightIds = right.Drug.ActiveSubstances.Select(substance => substance.Id).ToHashSet();

                List<InteractionExpanded> pairRows = bulk.DrugDrugRows
                    .Where(row =>
                        leftIds.Contains(row.PartyIdA) && rightIds.Contains(row.PartyIdB) ||
                        rightIds.Contains(row.PartyIdA) && leftIds.Contains(row.PartyIdB))
                    .ToList();

                bool bothOral = left.Form.IsOral && right.Form.IsOral;

                List<SubstancePairInteractionRowReadModel> forwardRows = MapDrugDrugPairRows(
                    pairRows,
                    leftIds,
                    rightIds,
                    bothOral,
                    bulk.InteractionById,
                    context.ActiveSubstanceNameById);

                var reverseRows = forwardRows
                    .Select(row => row with
                    {
                        CurrentActiveSubstanceId = row.ComparedActiveSubstanceId,
                        CurrentActiveSubstanceName = row.ComparedActiveSubstanceName,
                        ComparedActiveSubstanceId = row.CurrentActiveSubstanceId,
                        ComparedActiveSubstanceName = row.CurrentActiveSubstanceName,
                        DisplayLine = DrugInteractionToolReportMapping.BuildSubstancePairDisplayLine(
                            row.ComparedActiveSubstanceName,
                            row.CurrentActiveSubstanceName,
                            row.DefaultText,
                            row.StomachText,
                            row.Severity)
                    })
                    .ToList();

                result[new CrossDrugPairKey(left.Form.Id, right.Form.Id)] = forwardRows;
                result[new CrossDrugPairKey(right.Form.Id, left.Form.Id)] = reverseRows;
            }
        }

        return result;
    }

    private static List<SubstancePairInteractionRowReadModel> MapDrugDrugPairRows(
        IReadOnlyCollection<InteractionExpanded> expandedRows,
        HashSet<Guid> currentSubstanceIds,
        HashSet<Guid> comparedSubstanceIds,
        bool includeStomachText,
        IReadOnlyDictionary<Guid, Interaction> interactionById,
        IReadOnlyDictionary<Guid, string> substanceNameById)
    {
        if (expandedRows.Count == 0)
        {
            return new List<SubstancePairInteractionRowReadModel>();
        }

        List<SubstancePairInteractionRowReadModel> rows = new();

        foreach (InteractionExpanded expanded in expandedRows.OrderBy(row => row.PartyIdA).ThenBy(row => row.PartyIdB))
        {
            if (!interactionById.TryGetValue(expanded.InteractionId, out Interaction? interaction))
            {
                continue;
            }

            Guid currentId;
            Guid comparedId;

            if (currentSubstanceIds.Contains(expanded.PartyIdA) && comparedSubstanceIds.Contains(expanded.PartyIdB))
            {
                currentId = expanded.PartyIdA;
                comparedId = expanded.PartyIdB;
            }
            else if (currentSubstanceIds.Contains(expanded.PartyIdB) && comparedSubstanceIds.Contains(expanded.PartyIdA))
            {
                currentId = expanded.PartyIdB;
                comparedId = expanded.PartyIdA;
            }
            else
            {
                continue;
            }

            string currentName = substanceNameById[currentId];
            string comparedName = substanceNameById[comparedId];
            string severity = interaction.InteractionSeverity.Name;
            string? stomachText = includeStomachText ? interaction.StomachText : null;
            string defaultText = interaction.DefaultText ?? string.Empty;

            rows.Add(new SubstancePairInteractionRowReadModel(
                CurrentActiveSubstanceId: currentId,
                CurrentActiveSubstanceName: currentName,
                ComparedActiveSubstanceId: comparedId,
                ComparedActiveSubstanceName: comparedName,
                InteractionId: interaction.Id,
                DefaultText: defaultText,
                StomachText: stomachText,
                Severity: severity,
                DisplayLine: DrugInteractionToolReportMapping.BuildSubstancePairDisplayLine(
                    currentName,
                    comparedName,
                    defaultText,
                    stomachText,
                    severity)));
        }

        return rows
            .OrderBy(row => row.CurrentActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ComparedActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.InteractionId)
            .ToList();
    }

    private static DrugInteractionSectionReadModel BuildPerDrugDrugSection(
        Guid currentFormId,
        ReportCompositionContext context,
        IReadOnlyDictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> crossDrugPairs)
    {
        List<ComparedDrugInteractionGroupReadModel> groups = new();
        int totalRows = 0;

        foreach (DrugFormSelection other in context.Selections.Where(selection => selection.Form.Id != currentFormId))
        {
            CrossDrugPairKey key = new(currentFormId, other.Form.Id);
            if (!crossDrugPairs.TryGetValue(key, out List<SubstancePairInteractionRowReadModel>? pairRows) || pairRows.Count == 0)
            {
                continue;
            }

            DrugTargetSummaryReadModel currentSummary = context.TargetSummariesByFormId[currentFormId];
            DrugTargetSummaryReadModel comparedSummary = context.TargetSummariesByFormId[other.Form.Id];

            groups.Add(new ComparedDrugInteractionGroupReadModel(
                GroupTitle: DrugInteractionToolReportMapping.BuildComparedDrugGroupTitle(currentSummary, comparedSummary),
                CurrentDrug: currentSummary,
                ComparedDrug: comparedSummary,
                RecordCount: pairRows.Count,
                PairRows: pairRows));

            totalRows += pairRows.Count;
        }

        return new DrugInteractionSectionReadModel(
            SectionCode: "specific_drug.drugs",
            RecordCount: totalRows,
            ComparedDrugGroups: groups
                .OrderBy(group => group.ComparedDrug.DrugSummaryText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(group => group.ComparedDrug.DrugFormId)
                .ToList());
    }

    private static AllDrugsInteractionsSectionReadModel? BuildGlobalSection(
        IReadOnlyList<DrugFormSelection> selections,
        ReportCompositionContext context,
        IReadOnlyDictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> crossDrugPairs)
    {
        List<GlobalDrugPairGroupReadModel> pairGroups = new();
        int totalRows = 0;

        for (int leftIndex = 0; leftIndex < selections.Count; leftIndex++)
        {
            for (int rightIndex = leftIndex + 1; rightIndex < selections.Count; rightIndex++)
            {
                DrugFormSelection left = selections[leftIndex];
                DrugFormSelection right = selections[rightIndex];
                CrossDrugPairKey key = new(left.Form.Id, right.Form.Id);

                if (!crossDrugPairs.TryGetValue(key, out List<SubstancePairInteractionRowReadModel>? rows) || rows.Count == 0)
                {
                    continue;
                }

                DrugTargetSummaryReadModel leftSummary = context.TargetSummariesByFormId[left.Form.Id];
                DrugTargetSummaryReadModel rightSummary = context.TargetSummariesByFormId[right.Form.Id];

                pairGroups.Add(new GlobalDrugPairGroupReadModel(
                    GroupTitle: DrugInteractionToolReportMapping.BuildGlobalPairGroupTitle(leftSummary, rightSummary),
                    LeftDrug: leftSummary,
                    RightDrug: rightSummary,
                    RecordCount: rows.Count,
                    PairRows: rows));

                totalRows += rows.Count;
            }
        }

        if (pairGroups.Count == 0)
        {
            return new AllDrugsInteractionsSectionReadModel(
                SectionCode: "all_drugs_interactions",
                RecordCount: 0,
                PairGroups: pairGroups);
        }

        return new AllDrugsInteractionsSectionReadModel(
            SectionCode: "all_drugs_interactions",
            RecordCount: totalRows,
            PairGroups: pairGroups
                .OrderBy(group => group.LeftDrug.DrugSummaryText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(group => group.RightDrug.DrugSummaryText, StringComparer.OrdinalIgnoreCase)
                .ToList());
    }

    private async Task<DrugDuplicationSectionReadModel> BuildDuplicationSectionAsync(
        ReportCompositionContext context,
        CancellationToken cancellationToken)
    {
        List<ExactDuplicationRowReadModel> exactRows = BuildExactDuplicationRows(context);
        List<GroupDuplicationRowReadModel> groupRows = await BuildGroupDuplicationRowsAsync(context, exactRows, cancellationToken);

        return new DrugDuplicationSectionReadModel(
            SectionCode: "drug_duplication",
            ExactActiveSubstanceDuplicates: DrugInteractionToolReportMapping.BuildSubsection(
                "drug_duplication.exact_active_substance",
                DrugInteractionToolHeadlines.ExactDuplication,
                exactRows),
            ActiveSubstanceGroupDuplicates: DrugInteractionToolReportMapping.BuildSubsection(
                "drug_duplication.active_substance_group",
                DrugInteractionToolHeadlines.GroupDuplication,
                groupRows));
    }

    private static List<ExactDuplicationRowReadModel> BuildExactDuplicationRows(ReportCompositionContext context)
    {
        Dictionary<Guid, Dictionary<Guid, DuplicationDrugSummaryReadModel>> drugsBySubstanceAndForm = new();

        foreach (DrugFormSelection selection in context.Selections)
        {
            DuplicationDrugSummaryReadModel summary = DrugInteractionToolReportMapping.MapDuplicationDrugSummary(
                selection.Drug,
                selection.Form);

            foreach (ActiveSubstance substance in selection.Drug.ActiveSubstances)
            {
                if (!drugsBySubstanceAndForm.TryGetValue(substance.Id, out Dictionary<Guid, DuplicationDrugSummaryReadModel>? byForm))
                {
                    byForm = new Dictionary<Guid, DuplicationDrugSummaryReadModel>();
                    drugsBySubstanceAndForm[substance.Id] = byForm;
                }

                byForm[selection.Form.Id] = summary;
            }
        }

        List<ExactDuplicationRowReadModel> rows = new();

        foreach ((Guid substanceId, Dictionary<Guid, DuplicationDrugSummaryReadModel> affectedByForm) in drugsBySubstanceAndForm
                     .Where(pair => pair.Value.Count > 1)
                     .OrderBy(pair => context.ActiveSubstanceNameById[pair.Key], StringComparer.OrdinalIgnoreCase))
        {
            string substanceName = context.ActiveSubstanceNameById[substanceId];
            IReadOnlyCollection<DuplicationDrugSummaryReadModel> distinctDrugs = affectedByForm.Values
                .GroupBy(drug => drug.DrugId)
                .Select(group => group.First())
                .OrderBy(drug => drug.OnlyDrugSummaryText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(drug => drug.DrugId)
                .ToList();

            rows.Add(new ExactDuplicationRowReadModel(
                ActiveSubstanceId: substanceId,
                ActiveSubstanceName: substanceName,
                AffectedDrugs: distinctDrugs,
                DisplayLine:
                $"Active substance: {substanceName} IS IN BOTH DRUGS WHICH IS CONSIDERED AS A DUPLICATION.",
                DuplicationKind: DuplicationKind.ExactActiveSubstance));
        }

        return rows;
    }

    private async Task<List<GroupDuplicationRowReadModel>> BuildGroupDuplicationRowsAsync(
        ReportCompositionContext context,
        IReadOnlyCollection<ExactDuplicationRowReadModel> exactRows,
        CancellationToken cancellationToken)
    {
        var exactSubstanceIds = exactRows.Select(row => row.ActiveSubstanceId).ToHashSet();
        var allSubstanceIds = context.ActiveSubstanceNameById.Keys.ToHashSet();

        List<ActiveSubstanceGroup> groups = await _activeSubstanceGroupRepository.ListByMemberIdsAsync(
            allSubstanceIds,
            cancellationToken);

        List<GroupDuplicationRowReadModel> rows = new();

        foreach (ActiveSubstanceGroup group in groups.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ThenBy(g => g.Id))
        {
            var memberIds = group.ActiveSubstances.Select(member => member.Id).ToHashSet();
            var affectedDrugs = context.Selections
                .Where(selection => selection.Drug.ActiveSubstances.Any(substance => memberIds.Contains(substance.Id)))
                .GroupBy(selection => selection.Form.Id)
                .Select(formGroup => DrugInteractionToolReportMapping.MapDuplicationDrugSummary(
                    formGroup.First().Drug,
                    formGroup.First().Form))
                .GroupBy(summary => summary.DrugId)
                .Select(drugGroup => drugGroup.First())
                .OrderBy(summary => summary.OnlyDrugSummaryText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(summary => summary.DrugId)
                .ToList();

            if (affectedDrugs.Count < 2)
            {
                continue;
            }

            bool onlyExactOverlap = context.Selections
                .Where(selection => selection.Drug.ActiveSubstances.Any(substance => memberIds.Contains(substance.Id)))
                .All(selection =>
                    selection.Drug.ActiveSubstances.All(substance =>
                        !memberIds.Contains(substance.Id) || exactSubstanceIds.Contains(substance.Id)));

            if (onlyExactOverlap && exactRows.Count > 0)
            {
                continue;
            }

            IReadOnlyCollection<GroupActiveSubstanceReadModel> groupActiveSubstances = group.ActiveSubstances
                .OrderBy(member => member.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.Id)
                .Select(member => new GroupActiveSubstanceReadModel(member.Id, member.ActiveSubstanceName))
                .ToList();

            IReadOnlyCollection<AffectedActiveSubstanceReadModel> affectedActiveSubstances = context.Selections
                .SelectMany(selection => selection.Drug.ActiveSubstances)
                .Where(substance => memberIds.Contains(substance.Id))
                .GroupBy(substance => substance.Id)
                .Select(substanceGroup => substanceGroup.First())
                .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(substance => substance.Id)
                .Select(substance => new AffectedActiveSubstanceReadModel(substance.Id, substance.ActiveSubstanceName))
                .ToList();

            rows.Add(new GroupDuplicationRowReadModel(
                ActiveSubstanceGroupId: group.Id,
                GroupName: group.Name,
                GroupActiveSubstances: groupActiveSubstances,
                AffectedActiveSubstances: affectedActiveSubstances,
                AffectedDrugs: affectedDrugs,
                DisplayLine:
                $"Active substance group: {group.Name} IS SHARED BETWEEN MULTIPLE SELECTED DRUGS.",
                DuplicationKind: DuplicationKind.ActiveSubstanceGroup));
        }

        return rows;
    }

    private async Task<Dictionary<Guid, AlcoholInteractionRowReadModel>> BuildAlcoholRowsAsync(
        ReportCompositionContext context,
        CancellationToken cancellationToken)
    {
        string defaultAlcoholText = (await _systemTextRepository.GetByKeyAsync(AlcoholDefaultTextKey, cancellationToken))?.Value
            ?? string.Empty;

        Dictionary<Guid, AlcoholInteractionRowReadModel> result = new();

        foreach ((Guid substanceId, ActiveSubstance substance) in context.ActiveSubstanceById)
        {
            if (!substance.HasAlcoholInteraction)
            {
                continue;
            }

            string defaultText = substance.AlcoholInteractionOverride?.AlcoholSpecialText ?? defaultAlcoholText;

            result[substanceId] = new AlcoholInteractionRowReadModel(
                ActiveSubstanceName: substance.ActiveSubstanceName,
                DefaultText: defaultText,
                SpecialText: substance.AlcoholInteractionOverride?.AlcoholSpecialText);
        }

        return result;
    }

    private static AlcoholInteractionSectionReadModel SliceAlcoholSection(
        DrugFormSelection selection,
        IReadOnlyDictionary<Guid, AlcoholInteractionRowReadModel> alcoholRows)
    {
        List<AlcoholInteractionRowReadModel> rows = selection.Drug.ActiveSubstances
            .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(substance => substance.Id)
            .Where(substance => alcoholRows.ContainsKey(substance.Id))
            .Select(substance => alcoholRows[substance.Id])
            .ToList();

        return new AlcoholInteractionSectionReadModel(
            HeadlineText: DrugInteractionToolHeadlines.AlcoholSection,
            Rows: rows);
    }

    private static RegisteredFoodInteractionSectionReadModel? SliceRegisteredFoodSection(
        DrugFormSelection selection,
        ReportCompositionContext context,
        BulkExpandedContext bulk,
        bool includeUserData,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        HashSet<Guid> takenNowFoodIds,
        IReadOnlyCollection<ClientMealTime> mealTimes)
    {
        bool showTakenNow = includeUserData;
        bool showAllOther = showAllFoodAndSupplementInteractions;

        if (!showTakenNow && !showAllOther && !(includeUserData && showOralGuidance && selection.Form.IsOral))
        {
            return null;
        }

        OralGuidanceSubsectionReadModel? oralGuidance = includeUserData && showOralGuidance && selection.Form.IsOral
            ? BuildOralGuidanceSubsection(selection, context, mealTimes)
            : null;

        ReportSubsectionReadModel<FoodInteractionRowReadModel>? takenNow = null;
        ReportSubsectionReadModel<FoodInteractionRowReadModel>? allOther = null;

        if (showTakenNow)
        {
            List<FoodInteractionRowReadModel> takenRows = SliceFoodRows(
                selection,
                context,
                bulk,
                takenNowFoodIds,
                includeAllCatalog: false);

            takenNow = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.foods.taken_now",
                DrugInteractionToolHeadlines.FoodTakenNow,
                takenRows);
        }

        if (showAllOther)
        {
            List<FoodInteractionRowReadModel> otherRows = SliceFoodRows(
                selection,
                context,
                bulk,
                includeUserData ? takenNowFoodIds : null,
                includeAllCatalog: true);

            allOther = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.foods.all_other",
                DrugInteractionToolHeadlines.FoodAllOtherRegistered,
                otherRows);
        }

        if (oralGuidance is null && takenNow is null && allOther is null)
        {
            return null;
        }

        return new RegisteredFoodInteractionSectionReadModel(
            SectionCode: "specific_drug.foods",
            OralGuidance: oralGuidance,
            TakenNow: takenNow,
            AllOther: allOther);
    }

    private static RegisteredSupplementInteractionSectionReadModel? SliceRegisteredSupplementSection(
        DrugFormSelection selection,
        ReportCompositionContext context,
        BulkExpandedContext bulk,
        bool includeUserData,
        bool showAllFoodAndSupplementInteractions,
        HashSet<Guid> takenNowSupplementIds)
    {
        bool showTakenNow = includeUserData;
        bool showAllOther = showAllFoodAndSupplementInteractions;

        if (!showTakenNow && !showAllOther)
        {
            return null;
        }

        ReportSubsectionReadModel<SupplementInteractionRowReadModel>? takenNow = null;
        ReportSubsectionReadModel<SupplementInteractionRowReadModel>? allOther = null;

        if (showTakenNow)
        {
            List<SupplementInteractionRowReadModel> takenRows = SliceSupplementRows(
                selection,
                context,
                bulk,
                takenNowSupplementIds,
                includeAllCatalog: false);

            takenNow = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.supplements.taken_now",
                DrugInteractionToolHeadlines.SupplementTakenNow,
                takenRows);
        }

        if (showAllOther)
        {
            List<SupplementInteractionRowReadModel> otherRows = SliceSupplementRows(
                selection,
                context,
                bulk,
                includeUserData ? takenNowSupplementIds : null,
                includeAllCatalog: true);

            allOther = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.supplements.all_other",
                DrugInteractionToolHeadlines.SupplementAllOtherRegistered,
                otherRows);
        }

        return new RegisteredSupplementInteractionSectionReadModel(
            SectionCode: "specific_drug.supplements",
            TakenNow: takenNow,
            AllOther: allOther);
    }

    private static OralGuidanceSubsectionReadModel? BuildOralGuidanceSubsection(
        DrugFormSelection selection,
        ReportCompositionContext context,
        IReadOnlyCollection<ClientMealTime> mealTimes)
    {
        string mealSummary = string.Join(
            "; ",
            mealTimes
                .OrderBy(meal => meal.IntakeTime)
                .ThenBy(meal => meal.MealName, StringComparer.OrdinalIgnoreCase)
                .Select(meal => $"{meal.MealName}: {meal.IntakeTime:HH:mm}"));

        var guidanceRows = selection.Drug.ActiveSubstances
            .Where(substance => selection.Form.IsOral)
            .Where(substance =>
                context.ActiveSubstanceById.TryGetValue(substance.Id, out ActiveSubstance? loaded)
                && loaded.OralMedicationIntakeGuidelines is not null)
            .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .Select(substance =>
            {
                ActiveSubstance loaded = context.ActiveSubstanceById[substance.Id];
                OralMedicationIntakeGuidelines guidelines = loaded.OralMedicationIntakeGuidelines!;
                string guidance = DrugInteractionToolReportMapping.FormatOralGuidanceText(guidelines);

                return new ActiveSubstanceOralGuidanceRowReadModel(
                    ActiveSubstanceId: substance.Id,
                    ActiveSubstanceName: substance.ActiveSubstanceName,
                    GuidanceText: guidance,
                    DisplayLine: $"{substance.ActiveSubstanceName}: {guidance}");
            })
            .ToList();

        if (guidanceRows.Count == 0)
        {
            return null;
        }

        return new OralGuidanceSubsectionReadModel(
            HeadlineText: DrugInteractionToolHeadlines.OralGuidanceMealBlock,
            MealIntakeSummary: mealSummary,
            ActiveSubstanceGuidanceRows: guidanceRows);
    }

    private static List<FoodInteractionRowReadModel> SliceFoodRows(
        DrugFormSelection selection,
        ReportCompositionContext context,
        BulkExpandedContext bulk,
        HashSet<Guid>? takenNowFoodIds,
        bool includeAllCatalog)
    {
        var substanceIds = selection.Drug.ActiveSubstances.Select(substance => substance.Id).ToHashSet();
        if (substanceIds.Count == 0)
        {
            return new List<FoodInteractionRowReadModel>();
        }

        var relevantRows = bulk.FoodRows.Where(row => substanceIds.Contains(row.PartyIdA));

        List<FoodInteractionRowReadModel> rows = new();

        foreach (InteractionExpanded expanded in relevantRows.OrderBy(row => row.PartyIdA).ThenBy(row => row.PartyIdB))
        {
            if (!bulk.InteractionById.TryGetValue(expanded.InteractionId, out Interaction? interaction))
            {
                continue;
            }

            InteractionParty? foodParty = interaction.Parties
                .FirstOrDefault(party => party.PartyType is InteractionType.Food or InteractionType.FoodGroup);

            Guid foodId;
            string foodName;

            if (foodParty?.PartyType == InteractionType.FoodGroup && foodParty.FoodGroupId.HasValue
                && bulk.FoodGroupById.TryGetValue(foodParty.FoodGroupId.Value, out FoodGroup? group))
            {
                foodId = group.Id;
                foodName = group.Name;
            }
            else if (bulk.FoodById.TryGetValue(expanded.PartyIdB, out Food? food))
            {
                foodId = food.Id;
                foodName = food.Name;
            }
            else
            {
                continue;
            }

            bool isTakenNow = takenNowFoodIds?.Contains(expanded.PartyIdB) == true
                || takenNowFoodIds?.Contains(foodId) == true;

            if (!includeAllCatalog && takenNowFoodIds is not null && !isTakenNow)
            {
                continue;
            }

            if (includeAllCatalog && takenNowFoodIds is not null && isTakenNow && takenNowFoodIds.Count > 0)
            {
                continue;
            }

            string substanceName = context.ActiveSubstanceNameById[expanded.PartyIdA];
            string? stomachText = selection.Form.IsOral ? interaction.StomachText : null;
            string defaultText = interaction.DefaultText ?? string.Empty;
            string severity = interaction.InteractionSeverity.Name;

            rows.Add(new FoodInteractionRowReadModel(
                ActiveSubstanceId: expanded.PartyIdA,
                ActiveSubstanceName: substanceName,
                FoodId: foodId,
                FoodName: foodName,
                InteractionId: interaction.Id,
                DefaultText: defaultText,
                StomachText: stomachText,
                Severity: severity,
                IsTakenNow: isTakenNow,
                DisplayLine: DrugInteractionToolReportMapping.BuildFoodOrSupplementDisplayLine(
                    substanceName,
                    foodName,
                    isTakenNow,
                    defaultText,
                    stomachText,
                    severity)));
        }

        return rows
            .GroupBy(row => (row.ActiveSubstanceId, row.FoodId, row.InteractionId))
            .Select(group => group.First())
            .OrderBy(row => row.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.FoodName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<SupplementInteractionRowReadModel> SliceSupplementRows(
        DrugFormSelection selection,
        ReportCompositionContext context,
        BulkExpandedContext bulk,
        HashSet<Guid>? takenNowSupplementIds,
        bool includeAllCatalog)
    {
        var substanceIds = selection.Drug.ActiveSubstances.Select(substance => substance.Id).ToHashSet();
        if (substanceIds.Count == 0)
        {
            return new List<SupplementInteractionRowReadModel>();
        }

        var relevantRows = bulk.SupplementRows.Where(row => substanceIds.Contains(row.PartyIdA));

        List<SupplementInteractionRowReadModel> rows = new();

        foreach (InteractionExpanded expanded in relevantRows.OrderBy(row => row.PartyIdA).ThenBy(row => row.PartyIdB))
        {
            if (!bulk.InteractionById.TryGetValue(expanded.InteractionId, out Interaction? interaction))
            {
                continue;
            }

            InteractionParty? supplementParty = interaction.Parties
                .FirstOrDefault(party => party.PartyType is InteractionType.Supplement or InteractionType.SupplementGroup);

            Guid supplementId;
            string supplementName;

            if (supplementParty?.PartyType == InteractionType.SupplementGroup
                && supplementParty.SupplementGroupId.HasValue
                && bulk.SupplementGroupById.TryGetValue(supplementParty.SupplementGroupId.Value, out SupplementGroup? group))
            {
                supplementId = group.Id;
                supplementName = group.Name ?? string.Empty;
            }
            else if (bulk.SupplementById.TryGetValue(expanded.PartyIdB, out Supplement? supplement))
            {
                supplementId = supplement.Id;
                supplementName = supplement.Name ?? string.Empty;
            }
            else
            {
                continue;
            }

            bool isTakenNow = takenNowSupplementIds?.Contains(expanded.PartyIdB) == true
                || takenNowSupplementIds?.Contains(supplementId) == true;

            if (!includeAllCatalog && takenNowSupplementIds is not null && !isTakenNow)
            {
                continue;
            }

            if (includeAllCatalog && takenNowSupplementIds is not null && isTakenNow && takenNowSupplementIds.Count > 0)
            {
                continue;
            }

            string substanceName = context.ActiveSubstanceNameById[expanded.PartyIdA];
            string? stomachText = selection.Form.IsOral ? interaction.StomachText : null;
            string defaultText = interaction.DefaultText ?? string.Empty;
            string severity = interaction.InteractionSeverity.Name;

            rows.Add(new SupplementInteractionRowReadModel(
                ActiveSubstanceId: expanded.PartyIdA,
                ActiveSubstanceName: substanceName,
                SupplementId: supplementId,
                SupplementName: supplementName,
                InteractionId: interaction.Id,
                DefaultText: defaultText,
                StomachText: stomachText,
                Severity: severity,
                IsTakenNow: isTakenNow,
                DisplayLine: DrugInteractionToolReportMapping.BuildFoodOrSupplementDisplayLine(
                    substanceName,
                    supplementName,
                    isTakenNow,
                    defaultText,
                    stomachText,
                    severity)));
        }

        return rows
            .GroupBy(row => (row.ActiveSubstanceId, row.SupplementId, row.InteractionId))
            .Select(group => group.First())
            .OrderBy(row => row.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.SupplementName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static AllergyIndexes BuildAllergyIndexes(
        List<ClientDrugAllergy> drugAllergies,
        List<DrugProblem> drugProblems)
    {
        var problemsByDrugFormId = drugProblems
            .Where(problem => problem.Source == DrugProblemSource.Database && problem.DrugFormId.HasValue)
            .GroupBy(problem => problem.DrugFormId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(problem => new SpecificDrugProblemRowReadModel(
                        ProblemDescription: problem.ProblemNote,
                        HeadlineText:
                        $"THIS DRUG WAS CHOSEN BY THE USER AS PROBLEMATIC AND HAS THIS DESCRIPTION OF A PROBLEM {problem.ProblemNote}"))
                    .ToList());

        var directAllergyByDrugFormId = drugAllergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database && allergy.DrugFormId.HasValue)
            .GroupBy(allergy => allergy.DrugFormId!.Value)
            .ToDictionary(
                group => group.Key,
                group => new SpecificDrugDirectAllergyRowReadModel(
                    ProblemDescription: group.First().Note ?? string.Empty,
                    HeadlineText:
                    $"THIS DRUG WAS MARKED BY THE USER AS ALLERGIC: {group.First().Note ?? string.Empty}"));

        Dictionary<Guid, List<ActiveSubstanceAllergyOverlapRowReadModel>> overlapBySubstanceId = new();

        foreach (ClientDrugAllergy allergy in drugAllergies.Where(allergy =>
                     allergy.Source == AllergyRecordSource.Database && allergy.DrugForm?.Drug is not null))
        {
            Drug sourceDrug = allergy.DrugForm!.Drug;
            string allergyDisplay = allergy.MapToDto().DisplayName;

            foreach (ActiveSubstance substance in sourceDrug.ActiveSubstances)
            {
                if (!overlapBySubstanceId.TryGetValue(substance.Id, out List<ActiveSubstanceAllergyOverlapRowReadModel>? list))
                {
                    list = new List<ActiveSubstanceAllergyOverlapRowReadModel>();
                    overlapBySubstanceId[substance.Id] = list;
                }

                list.Add(new ActiveSubstanceAllergyOverlapRowReadModel(
                    ActiveSubstanceId: substance.Id,
                    ActiveSubstanceName: substance.ActiveSubstanceName,
                    SourceAllergyDrugDisplay: allergyDisplay,
                    UserNote: allergy.Note,
                    HeadlineText:
                    $"THIS ACTIVE SUBSTANCE WAS CHOSEN AS THE ALLERGIC BY THE USER IN ANOTHER DRUG: {allergyDisplay}",
                    DisplayLine:
                    $"{substance.ActiveSubstanceName} <= allergic via {allergyDisplay} | {allergy.Note}"));
            }
        }

        var dedupedOverlapBySubstanceId = overlapBySubstanceId.ToDictionary(
            pair => pair.Key,
            pair => pair.Value
                .GroupBy(row => row.SourceAllergyDrugDisplay)
                .Select(group => group.First())
                .ToList());

        return new AllergyIndexes(problemsByDrugFormId, directAllergyByDrugFormId, dedupedOverlapBySubstanceId);
    }

    private static SpecificDrugProblemsSectionReadModel? SliceProblems(Guid drugFormId, AllergyIndexes indexes)
    {
        if (!indexes.ProblemsByDrugFormId.TryGetValue(drugFormId, out List<SpecificDrugProblemRowReadModel>? rows)
            || rows.Count == 0)
        {
            return null;
        }

        return new SpecificDrugProblemsSectionReadModel(
            SectionCode: "specific_drug.problems",
            RecordCount: rows.Count,
            Rows: rows);
    }

    private static SpecificDrugAllergiesSectionReadModel? SliceAllergies(
        DrugFormSelection selection,
        AllergyIndexes indexes)
    {
        SpecificDrugDirectAllergyRowReadModel? direct = indexes.DirectAllergyByDrugFormId
            .GetValueOrDefault(selection.Form.Id);

        var overlapRows = selection.Drug.ActiveSubstances
            .SelectMany(substance => indexes.OverlapBySubstanceId.GetValueOrDefault(substance.Id, EmptyOverlapRows))
            .OrderBy(row => row.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (direct is null && overlapRows.Count == 0)
        {
            return null;
        }

        return new SpecificDrugAllergiesSectionReadModel(
            SectionCode: "specific_drug.allergies",
            RecordCount: overlapRows.Count + (direct is null ? 0 : 1),
            DirectDrugAllergy: direct,
            ActiveSubstanceOverlapRows: overlapRows);
    }

    private static UserInfoSectionReadModel BuildUserInfoSection(
        UserMedicalBundle bundle,
        IReadOnlyCollection<ClientRegularDrug> regularDrugs,
        HashSet<Guid> takenNowFoodIds,
        HashSet<Guid> takenNowSupplementIds)
    {
        List<MainWarningReadModel> mainWarnings = BuildMainWarnings(bundle.ReproductiveHealth);
        HealthConditionsGroupReadModel healthConditions = BuildHealthConditionsGroup(bundle.HealthConditions);
        ProblematicDrugsGroupReadModel problematicDrugs = BuildProblematicDrugsGroup(
            bundle.DrugProblems,
            regularDrugs);
        AllergiesGroupReadModel allergies = BuildAllergiesGroup(
            bundle,
            regularDrugs,
            takenNowFoodIds,
            takenNowSupplementIds);

        return new UserInfoSectionReadModel(
            MainWarnings: mainWarnings,
            HealthConditions: healthConditions,
            ProblematicDrugs: problematicDrugs,
            Allergies: allergies);
    }

    private static List<MainWarningReadModel> BuildMainWarnings(ClientReproductiveHealthProfile? profile)
    {
        if (profile is null)
        {
            return new List<MainWarningReadModel>();
        }

        List<MainWarningReadModel> warnings = new();

        if (profile.CurrentlyPregnant)
        {
            warnings.Add(new MainWarningReadModel(
                WarningCode: "currently_pregnant",
                DefaultText: DrugInteractionToolHeadlines.MainWarningCurrentlyPregnant));
        }

        if (profile.PlanningToGetPregnant)
        {
            warnings.Add(new MainWarningReadModel(
                WarningCode: "planning_to_get_pregnant",
                DefaultText: DrugInteractionToolHeadlines.MainWarningPlanningPregnancy));
        }

        return warnings;
    }

    private static HealthConditionsGroupReadModel BuildHealthConditionsGroup(List<UserHealthCondition> rows)
    {
        var databaseRows = rows
            .Where(row => row.Source == HealthConditionSource.Database && row.HealthCondition is not null)
            .Select(row => new HealthConditionRowReadModel(
                HealthConditionId: row.HealthConditionId!.Value,
                HealthConditionName: row.HealthCondition!.HealthConditionName,
                UserNote: row.Note,
                DisplayLine: $"{row.HealthCondition.HealthConditionName} | {row.Note}"))
            .ToList();

        var customRows = rows
            .Where(row => row.Source == HealthConditionSource.Custom)
            .Select(row => new CustomHealthConditionRowReadModel(
                CustomName: row.CustomHealthConditionName ?? string.Empty,
                UserNote: row.Note,
                DisplayLine: $"{row.CustomHealthConditionName} | {row.Note}"))
            .ToList();

        return new HealthConditionsGroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "health_conditions.from_database",
                DrugInteractionToolHeadlines.HealthConditionsDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "health_conditions.custom",
                DrugInteractionToolHeadlines.HealthConditionsCustom,
                customRows));
    }

    private static ProblematicDrugsGroupReadModel BuildProblematicDrugsGroup(
        List<DrugProblem> problems,
        IReadOnlyCollection<ClientRegularDrug> regularDrugs)
    {
        var databaseRows = problems
            .Where(problem => problem.Source == DrugProblemSource.Database && problem.DrugForm?.Drug is not null)
            .Select(problem =>
            {
                Drug drug = problem.DrugForm!.Drug;
                DrugForm drugForm = problem.DrugForm;
                string drugSummaryText = DrugInteractionToolReportMapping.BuildDrugSummaryText(drug, drugForm);

                return new ProblematicDrugRowReadModel(
                    DrugId: drug.Id,
                    DrugFormId: drugForm.Id,
                    DrugSummaryText: drugSummaryText,
                    UserNote: problem.ProblemNote,
                    DisplayLine: $"{drugSummaryText} | {problem.ProblemNote}");
            })
            .ToList();

        var customRows = problems
            .Where(problem => problem.Source == DrugProblemSource.Custom)
            .Select(problem =>
            {
                string? otherCustomData = DrugInteractionToolReportMapping.BuildOtherCustomData(
                    problem.CustomActiveSubstances,
                    problem.CustomDosage,
                    problem.CustomFormulationName);
                string customCommercialName = problem.CustomDrugName ?? string.Empty;
                string displayLine = string.Join(
                    " | ",
                    new[] { customCommercialName, otherCustomData, problem.ProblemNote }
                        .Where(part => !string.IsNullOrWhiteSpace(part)));

                return new CustomProblematicDrugRowReadModel(
                    CustomCommercialName: customCommercialName,
                    OtherCustomData: string.IsNullOrWhiteSpace(otherCustomData) ? null : otherCustomData,
                    UserNote: problem.ProblemNote,
                    DisplayLine: displayLine);
            })
            .ToList();

        return new ProblematicDrugsGroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "problematic_drugs.from_database",
                DrugInteractionToolHeadlines.ProblematicDrugsDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "problematic_drugs.custom",
                DrugInteractionToolHeadlines.ProblematicDrugsCustom,
                customRows),
            TakenNowOverlap: DrugTakenNowProblemOverlapBuilder.BuildSubsection(regularDrugs, problems));
    }

    private static AllergiesGroupReadModel BuildAllergiesGroup(
        UserMedicalBundle bundle,
        IReadOnlyCollection<ClientRegularDrug> regularDrugs,
        HashSet<Guid> takenFoodIds,
        HashSet<Guid> takenSupplementIds)
    {
        return new AllergiesGroupReadModel(
            DrugAllergies: BuildDrugAllergySubgroup(bundle.DrugAllergies, regularDrugs),
            SupplementAllergies: BuildSupplementAllergySubgroup(bundle.SupplementAllergies, takenSupplementIds),
            FoodAllergies: BuildFoodAllergySubgroup(bundle.FoodAllergies, takenFoodIds),
            OtherAllergies: BuildOtherAllergySubgroup(bundle.AllergySummary, bundle.OtherAllergies));
    }

    private static AllergyDrugSubgroupReadModel BuildDrugAllergySubgroup(
        IReadOnlyCollection<ClientDrugAllergy> allergies,
        IReadOnlyCollection<ClientRegularDrug> regularDrugs)
    {
        var databaseRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database && allergy.DrugForm?.Drug is not null)
            .Select(allergy =>
            {
                Drug drug = allergy.DrugForm!.Drug;
                DrugForm drugForm = allergy.DrugForm;
                string drugSummaryText = DrugInteractionToolReportMapping.BuildDrugSummaryText(drug, drugForm);

                return new DrugAllergyRowReadModel(
                    DrugId: drug.Id,
                    DrugFormId: drugForm.Id,
                    DrugSummaryText: drugSummaryText,
                    UserNote: allergy.Note,
                    DisplayLine: $"{drugSummaryText} | {allergy.Note}");
            })
            .ToList();

        var customRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Custom)
            .Select(allergy =>
            {
                string? otherCustomData = DrugInteractionToolReportMapping.BuildOtherCustomData(
                    allergy.CustomActiveSubstances,
                    allergy.CustomDosage,
                    allergy.CustomFormulationName);
                string customCommercialName = allergy.CustomDrugName ?? string.Empty;
                string displayLine = string.Join(
                    " | ",
                    new[] { customCommercialName, otherCustomData, allergy.Note }
                        .Where(part => !string.IsNullOrWhiteSpace(part)));

                return new CustomDrugAllergyRowReadModel(
                    CustomCommercialName: customCommercialName,
                    OtherCustomData: string.IsNullOrWhiteSpace(otherCustomData) ? null : otherCustomData,
                    UserNote: allergy.Note,
                    DisplayLine: displayLine);
            })
            .ToList();

        return new AllergyDrugSubgroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.drug.from_database",
                DrugInteractionToolHeadlines.DrugAllergiesDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.drug.custom",
                DrugInteractionToolHeadlines.DrugAllergiesCustom,
                customRows),
            TakenNowOverlap: DrugTakenNowAllergyOverlapBuilder.BuildSubsection(regularDrugs, allergies));
    }

    private static AllergyFoodSubgroupReadModel BuildFoodAllergySubgroup(
        IReadOnlyCollection<ClientFoodAllergy> allergies,
        HashSet<Guid> takenFoodIds)
    {
        var databaseRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database && allergy.Food is not null)
            .Select(allergy => new FoodCatalogAllergyRowReadModel(
                FoodId: allergy.Food!.Id,
                Name: allergy.Food.Name,
                AliasNames: allergy.Food.Aliases.ToList(),
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.Food.Name} | {allergy.Note}"))
            .ToList();

        var customRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Custom)
            .Select(allergy => new CustomFoodAllergyRowReadModel(
                Name: allergy.CustomFoodName ?? string.Empty,
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.CustomFoodName} | {allergy.Note}"))
            .ToList();

        var overlapRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database
                && allergy.FoodId.HasValue
                && takenFoodIds.Contains(allergy.FoodId.Value))
            .Select(allergy => new FoodTakenNowAllergyOverlapRowReadModel(
                RegularFoodId: allergy.FoodId!.Value,
                RegularIntakeName: allergy.Food!.Name,
                AliasNames: allergy.Food.Aliases.ToList(),
                AllergyNote: allergy.Note,
                DisplayLine: $"{allergy.Food.Name} | {allergy.Note}"))
            .ToList();

        return new AllergyFoodSubgroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.food.from_database",
                DrugInteractionToolHeadlines.FoodAllergiesDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.food.custom",
                DrugInteractionToolHeadlines.FoodAllergiesCustom,
                customRows),
            TakenNowOverlap: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.food.taken_now_overlap",
                DrugInteractionToolHeadlines.FoodAllergiesTakenNowOverlap,
                overlapRows));
    }

    private static AllergySupplementSubgroupReadModel BuildSupplementAllergySubgroup(
        IReadOnlyCollection<ClientSupplementAllergy> allergies,
        HashSet<Guid> takenSupplementIds)
    {
        var databaseRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database && allergy.Supplement is not null)
            .Select(allergy => new SupplementCatalogAllergyRowReadModel(
                SupplementId: allergy.Supplement!.Id,
                Name: allergy.Supplement.Name,
                AliasNames: allergy.Supplement.Aliases.ToList(),
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.Supplement.Name} | {allergy.Note}"))
            .ToList();

        var customRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Custom)
            .Select(allergy => new CustomSupplementAllergyRowReadModel(
                Name: allergy.CustomSupplementName ?? string.Empty,
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.CustomSupplementName} | {allergy.Note}"))
            .ToList();

        var overlapRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database
                && allergy.SupplementId.HasValue
                && takenSupplementIds.Contains(allergy.SupplementId.Value))
            .Select(allergy => new SupplementTakenNowAllergyOverlapRowReadModel(
                RegularSupplementId: allergy.SupplementId!.Value,
                RegularIntakeName: allergy.Supplement!.Name,
                AliasNames: allergy.Supplement.Aliases.ToList(),
                AllergyNote: allergy.Note,
                DisplayLine: $"{allergy.Supplement.Name} | {allergy.Note}"))
            .ToList();

        return new AllergySupplementSubgroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.supplement.from_database",
                DrugInteractionToolHeadlines.SupplementAllergiesDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.supplement.custom",
                DrugInteractionToolHeadlines.SupplementAllergiesCustom,
                customRows),
            TakenNowOverlap: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.supplement.taken_now_overlap",
                DrugInteractionToolHeadlines.SupplementAllergiesTakenNowOverlap,
                overlapRows));
    }

    private static AllergyOtherSubgroupReadModel BuildOtherAllergySubgroup(
        ClientAllergySummary? summary,
        IReadOnlyCollection<ClientOtherAllergy> customAllergies)
    {
        List<OtherAllergyCatalogRowReadModel> databaseRows = new();

        if (summary?.PollenDustMitesAllergyStatus == AllergyAnswerState.Yes)
        {
            databaseRows.Add(new OtherAllergyCatalogRowReadModel(
                Name: "Pollen / dust / mites",
                DescriptionFromDb: "User declared pollen, dust, or mites allergy.",
                DisplayLine: "Pollen / dust / mites allergy declared."));
        }

        if (summary?.InsectVenomAllergyStatus == AllergyAnswerState.Yes)
        {
            databaseRows.Add(new OtherAllergyCatalogRowReadModel(
                Name: "Insect venom",
                DescriptionFromDb: "User declared insect venom allergy.",
                DisplayLine: "Insect venom allergy declared."));
        }

        if (summary?.LatexAllergyStatus == AllergyAnswerState.Yes)
        {
            databaseRows.Add(new OtherAllergyCatalogRowReadModel(
                Name: "Latex",
                DescriptionFromDb: "User declared latex allergy.",
                DisplayLine: "Latex allergy declared."));
        }

        var customRows = customAllergies
            .Select(allergy => new CustomOtherAllergyRowReadModel(
                Name: allergy.AllergyName,
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.AllergyName} | {allergy.Note}"))
            .ToList();

        return new AllergyOtherSubgroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.other.from_database",
                DrugInteractionToolHeadlines.OtherAllergiesDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.other.custom",
                DrugInteractionToolHeadlines.OtherAllergiesCustom,
                customRows));
    }

    private async Task<UserMedicalBundle> LoadUserMedicalBundleAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<UserHealthCondition> healthConditions = await LoadAllUserHealthConditionsAsync(userId, cancellationToken);
        List<DrugProblem> drugProblems = await LoadAllDrugProblemsAsync(userId, cancellationToken);
        List<ClientDrugAllergy> drugAllergies = await LoadAllDrugAllergiesAsync(userId, cancellationToken);
        List<ClientFoodAllergy> foodAllergies = await LoadAllFoodAllergiesAsync(userId, cancellationToken);
        List<ClientSupplementAllergy> supplementAllergies = await LoadAllSupplementAllergiesAsync(userId, cancellationToken);
        List<ClientOtherAllergy> otherAllergies = await LoadAllOtherAllergiesAsync(userId, cancellationToken);
        ClientAllergySummary? allergySummary = await _clientAllergySummaryRepository.GetByUserIdAsync(userId, cancellationToken);
        ClientReproductiveHealthProfile? reproductiveHealth =
            await _clientReproductiveHealthProfileRepository.GetTrackedByUserIdAsync(userId, cancellationToken);

        return new UserMedicalBundle(
            healthConditions,
            drugProblems,
            drugAllergies,
            foodAllergies,
            supplementAllergies,
            otherAllergies,
            allergySummary,
            reproductiveHealth);
    }

    private async Task<List<ClientRegularDrug>> LoadAllLiveRegularDrugsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await LoadAllPagesAsync(
            (skip, take, token) => _clientRegularDrugRepository.SearchLiveByUserIdAsync(
                userId,
                drugNameQuery: null,
                startDate: null,
                skip,
                take,
                token),
            cancellationToken);
    }

    private async Task<List<ClientRegularFood>> LoadAllLiveRegularFoodsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await LoadAllPagesAsync(
            (skip, take, token) => _clientRegularFoodRepository.SearchLiveByUserIdAsync(
                userId,
                foodNameQuery: null,
                startDate: null,
                skip,
                take,
                token),
            cancellationToken);
    }

    private async Task<List<ClientRegularSupplement>> LoadAllLiveRegularSupplementsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await LoadAllPagesAsync(
            (skip, take, token) => _clientRegularSupplementRepository.SearchLiveByUserIdAsync(
                userId,
                nameQuery: null,
                startDate: null,
                skip,
                take,
                token),
            cancellationToken);
    }

    private async Task<List<UserHealthCondition>> LoadAllUserHealthConditionsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await LoadAllPagesAsync(
            (skip, take, token) => _userHealthConditionRepository.SearchAsync(userId, null, skip, take, token),
            cancellationToken);
    }

    private async Task<List<DrugProblem>> LoadAllDrugProblemsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await LoadAllPagesAsync(
            (skip, take, token) => _drugProblemRepository.SearchByUserIdAsync(userId, skip, take, token),
            cancellationToken);
    }

    private async Task<List<ClientDrugAllergy>> LoadAllDrugAllergiesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await LoadAllPagesAsync(
            (skip, take, token) => _clientDrugAllergyRepository.SearchAsync(userId, skip, take, token),
            cancellationToken);
    }

    private async Task<List<ClientFoodAllergy>> LoadAllFoodAllergiesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await LoadAllPagesAsync(
            (skip, take, token) => _clientFoodAllergyRepository.SearchAsync(userId, skip, take, token),
            cancellationToken);
    }

    private async Task<List<ClientSupplementAllergy>> LoadAllSupplementAllergiesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await LoadAllPagesAsync(
            (skip, take, token) => _clientSupplementAllergyRepository.SearchAsync(userId, skip, take, token),
            cancellationToken);
    }

    private async Task<List<ClientOtherAllergy>> LoadAllOtherAllergiesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await LoadAllPagesAsync(
            (skip, take, token) => _clientOtherAllergyRepository.SearchAsync(userId, skip, take, token),
            cancellationToken);
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
