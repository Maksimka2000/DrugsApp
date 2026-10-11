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
    private const string AlcoholDefaultTextKey = "activeSubstance.alcohol.defaultText";

    private sealed record DrugFormSelection(
        Drug Drug,
        DrugForm Form,
        SelectedDrugSource? Source,
        RegularDrugIntakeReadModel? RegularIntake = null);

    /// <summary>
    /// Single bulk expanded-interaction load shared by cross-drug, food, and supplement slicing.
    /// </summary>
    private sealed class BulkExpandedContext
    {
        public required List<InteractionExpanded> DrugDrugRows { get; init; }

        public required ILookup<Guid, InteractionExpanded> FoodRowsBySubstanceId { get; init; }

        public required ILookup<Guid, InteractionExpanded> SupplementRowsBySubstanceId { get; init; }

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
    /// <param name="selectedSupplementIds">Catalog supplements selected for the analysis.</param>
    /// <param name="selectedFoodIds">Catalog foods selected for the analysis.</param>
    /// <param name="showPossibleInteractions">Whether catalog foods and supplements that are not selected are analysed.</param>
    /// <returns>Report read model or error collection.</returns>
    public async Task<ErrorOr<PublicDrugInteractionToolReportReadModel>> ComputePublicReportAsync(
        IReadOnlyCollection<Guid> manualDrugFormIds,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        CancellationToken cancellationToken,
        IReadOnlyCollection<Guid>? selectedSupplementIds = null,
        IReadOnlyCollection<Guid>? selectedFoodIds = null,
        bool showPossibleInteractions = true)
    {
        ErrorOr<IReadOnlyList<DrugFormSelection>> selectionsResult = await LoadManualSelectionsAsync(
            manualDrugFormIds,
            source: null,
            cancellationToken);

        if (selectionsResult.IsError)
        {
            return selectionsResult.Errors;
        }

        ErrorOr<SelectedCatalogItems> selectedItemsResult = await LoadSelectedCatalogItemsAsync(
            selectedSupplementIds,
            selectedFoodIds,
            cancellationToken);

        if (selectedItemsResult.IsError)
        {
            return selectedItemsResult.Errors;
        }

        return await BuildPublicReportAsync(
            selectionsResult.Value,
            manualDrugFormIds.Count,
            showAllDrugsInteractions,
            showOralGuidance,
            showAllFoodAndSupplementInteractions,
            selectedItemsResult.Value.Supplements,
            selectedItemsResult.Value.Foods,
            showPossibleInteractions,
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
    /// <param name="selectedSupplementIds">Catalog supplements selected for the analysis (provider: for prescription).</param>
    /// <param name="selectedFoodIds">Catalog foods selected for the analysis (provider: for prescription).</param>
    /// <param name="audience">Report audience; only changes the wording of the selected headlines.</param>
    /// <param name="showPossibleInteractions">Whether catalog foods and supplements that are neither taken nor selected are analysed.</param>
    /// <returns>Registered report read model or error collection.</returns>
    public async Task<ErrorOr<RegisteredDrugInteractionToolReportReadModel>> ComputeRegisteredReportAsync(
        Guid userId,
        IReadOnlyCollection<Guid> manualDrugFormIds,
        bool includeUserData,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        CancellationToken cancellationToken,
        IReadOnlyCollection<Guid>? selectedSupplementIds = null,
        IReadOnlyCollection<Guid>? selectedFoodIds = null,
        ReportAudience audience = ReportAudience.Registered,
        bool showPossibleInteractions = true)
    {
        ErrorOr<IReadOnlyList<DrugFormSelection>> selectionsResult = await LoadManualSelectionsAsync(
            manualDrugFormIds,
            source: SelectedDrugSource.Manual,
            cancellationToken);

        if (selectionsResult.IsError)
        {
            return selectionsResult.Errors;
        }

        ErrorOr<SelectedCatalogItems> selectedItemsResult = await LoadSelectedCatalogItemsAsync(
            selectedSupplementIds,
            selectedFoodIds,
            cancellationToken);

        if (selectedItemsResult.IsError)
        {
            return selectedItemsResult.Errors;
        }

        IReadOnlyList<Supplement> selectedSupplements = selectedItemsResult.Value.Supplements;
        IReadOnlyList<Food> selectedFoods = selectedItemsResult.Value.Foods;
        HashSet<Guid> selectedSupplementIdSet = selectedSupplements.Select(item => item.Id).ToHashSet();
        HashSet<Guid> selectedFoodIdSet = selectedFoods.Select(item => item.Id).ToHashSet();

        var effectiveSelections = selectionsResult.Value.ToList();

        UserInfoSectionReadModel? userInfoSection = null;
        HashSet<Guid> takenNowFoodIds = new();
        HashSet<Guid> takenNowSupplementIds = new();
        List<ClientRegularFood> regularFoods = new();
        List<ClientRegularSupplement> regularSupplements = new();
        Dictionary<Guid, IReadOnlyList<OwnSupplementSource>> ownSupplementSourcesBySupplementId = new();
        AllergyIndexes allergyIndexes = new(new(), new(), new());

        if (includeUserData)
        {
            List<ClientRegularDrug> regularDrugs = await _clientRegularDrugRepository.ListActiveByUserIdAsync(
                userId,
                cancellationToken);

            List<DrugFormIntake> plannedDrugs = BuildPlannedDrugIntakes(selectionsResult.Value, regularDrugs);

            effectiveSelections = MergeRegularDrugs(effectiveSelections, regularDrugs);

            regularFoods = await _clientRegularFoodRepository.ListActiveByUserIdAsync(userId, cancellationToken);
            takenNowFoodIds = regularFoods.Select(row => row.FoodId).ToHashSet();

            regularSupplements = await _clientRegularSupplementRepository.ListActiveByUserIdAsync(
                userId,
                cancellationToken);
            takenNowSupplementIds = regularSupplements
                .Where(row => row.Source == RegularSupplementSource.Database && row.SupplementId.HasValue)
                .Select(row => row.SupplementId!.Value)
                .ToHashSet();
            ownSupplementSourcesBySupplementId =
                OwnSupplementMembershipBuilder.BuildSourcesBySupplementId(regularSupplements);

            UserMedicalBundle bundle = await LoadUserMedicalBundleAsync(userId, cancellationToken);
            allergyIndexes = DrugAllergyProblemIndexBuilder.BuildAllergyIndexes(bundle.DrugAllergies, bundle.DrugProblems);
            userInfoSection = UserInfoSectionBuilder.BuildUserInfoSection(
                bundle,
                regularDrugs,
                plannedDrugs,
                takenNowFoodIds,
                takenNowSupplementIds,
                ownSupplementSourcesBySupplementId,
                selectedFoodIdSet,
                selectedSupplementIdSet,
                includeFoodAndSupplementAllergies: showAllFoodAndSupplementInteractions);
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
            ownSupplementSourcesBySupplementId,
            regularSupplements,
            regularFoods,
            selectedSupplements,
            selectedFoods,
            audience,
            showPossibleInteractions,
            allergyIndexes,
            cancellationToken);
    }

    private sealed record SelectedCatalogItems(IReadOnlyList<Supplement> Supplements, IReadOnlyList<Food> Foods);

    private async Task<ErrorOr<SelectedCatalogItems>> LoadSelectedCatalogItemsAsync(
        IReadOnlyCollection<Guid>? supplementIds,
        IReadOnlyCollection<Guid>? foodIds,
        CancellationToken cancellationToken)
    {
        List<Guid> distinctSupplementIds = (supplementIds ?? []).Distinct().ToList();
        List<Guid> distinctFoodIds = (foodIds ?? []).Distinct().ToList();

        List<Supplement> loadedSupplements = distinctSupplementIds.Count == 0
            ? new List<Supplement>()
            : await _supplementRepository.GetByIdsAsync(distinctSupplementIds, cancellationToken);
        List<Food> loadedFoods = distinctFoodIds.Count == 0
            ? new List<Food>()
            : await _foodRepository.GetByIdsAsync(distinctFoodIds, cancellationToken);

        Dictionary<Guid, Supplement> supplementById = loadedSupplements.ToDictionary(item => item.Id);
        Dictionary<Guid, Food> foodById = loadedFoods.ToDictionary(item => item.Id);

        Guid? missingSupplementId = distinctSupplementIds.Cast<Guid?>()
            .FirstOrDefault(id => !supplementById.ContainsKey(id!.Value));
        if (missingSupplementId.HasValue)
        {
            return Error.Validation(
                code: "DrugInteractionTool.SupplementNotFound",
                description: "One or more selected supplements were not found.",
                metadata: new Dictionary<string, object>
                {
                    ["missingResourceIds"] = missingSupplementId.Value.ToString()
                });
        }

        Guid? missingFoodId = distinctFoodIds.Cast<Guid?>()
            .FirstOrDefault(id => !foodById.ContainsKey(id!.Value));
        if (missingFoodId.HasValue)
        {
            return Error.Validation(
                code: "DrugInteractionTool.FoodNotFound",
                description: "One or more selected foods were not found.",
                metadata: new Dictionary<string, object>
                {
                    ["missingResourceIds"] = missingFoodId.Value.ToString()
                });
        }

        return new SelectedCatalogItems(
            distinctSupplementIds.Select(id => supplementById[id]).ToList(),
            distinctFoodIds.Select(id => foodById[id]).ToList());
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
        IReadOnlyList<Supplement> selectedSupplements,
        IReadOnlyList<Food> selectedFoods,
        bool showPossibleInteractions,
        CancellationToken cancellationToken)
    {
        ReportCompositionContext context = BuildCompositionContext(selections);

        HashSet<Guid> selectedSupplementIdSet = selectedSupplements.Select(item => item.Id).ToHashSet();
        HashSet<Guid> selectedFoodIdSet = selectedFoods.Select(item => item.Id).ToHashSet();
        bool includePossible = showAllFoodAndSupplementInteractions && showPossibleInteractions;

        BulkExpandedContext bulk = await BuildBulkExpandedContextAsync(
            context,
            includeAllFood: includePossible,
            takenNowFoodIds: showAllFoodAndSupplementInteractions && selectedFoodIdSet.Count > 0 ? selectedFoodIdSet : null,
            includeAllSupplements: includePossible,
            takenNowSupplementIds: showAllFoodAndSupplementInteractions && selectedSupplementIdSet.Count > 0
                ? selectedSupplementIdSet
                : null,
            cancellationToken);

        Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> crossDrugPairs =
            BuildCrossDrugPairs(context, bulk);

        DrugDuplicationSectionReadModel duplicationSection = await BuildDuplicationSectionAsync(
            context,
            cancellationToken,
            selectedSupplements: selectedSupplements,
            includeSupplementDuplication: showAllFoodAndSupplementInteractions);
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
                ReportSubsectionReadModel<FoodInteractionRowReadModel>? allOtherFoods = null;
                ReportSubsectionReadModel<FoodInteractionRowReadModel>? selectedFoodRows = null;

                if (includePossible)
                {
                    List<FoodInteractionRowReadModel> foodRows = SliceFoodRows(
                        selection,
                        context,
                        bulk,
                        takenNowFoodIds: null,
                        includeAllCatalog: true,
                        scopeIds: selectedFoodIdSet.Count > 0 ? selectedFoodIdSet : null);

                    allOtherFoods = DrugInteractionToolReportMapping.BuildSubsection(
                        "specific_drug.foods.all_other",
                        DrugInteractionToolHeadlines.FoodAllOtherPublic,
                        foodRows);
                }

                if (selectedFoodIdSet.Count > 0)
                {
                    selectedFoodRows = DrugInteractionToolReportMapping.BuildSubsection(
                        "specific_drug.foods.selected",
                        DrugInteractionToolHeadlines.FoodSelected,
                        SliceFoodRows(
                            selection,
                            context,
                            bulk,
                            takenNowFoodIds: null,
                            includeAllCatalog: false,
                            scopeIds: selectedFoodIdSet,
                            source: DuplicationDrugSource.Selected));
                }

                foods = new PublicFoodInteractionSectionReadModel(
                    SectionCode: "specific_drug.foods",
                    AllOther: allOtherFoods,
                    Selected: selectedFoodRows);
            }

            if (showAllFoodAndSupplementInteractions)
            {
                ReportSubsectionReadModel<SupplementInteractionRowReadModel>? allOtherSupplements = null;
                ReportSubsectionReadModel<SupplementInteractionRowReadModel>? selectedSupplementRows = null;

                if (includePossible)
                {
                    List<SupplementInteractionRowReadModel> supplementRows = SliceSupplementRows(
                        selection,
                        context,
                        bulk,
                        takenNowSupplementIds: null,
                        includeAllCatalog: true,
                        scopeIds: selectedSupplementIdSet.Count > 0 ? selectedSupplementIdSet : null);

                    allOtherSupplements = DrugInteractionToolReportMapping.BuildSubsection(
                        "specific_drug.supplements.all_other",
                        DrugInteractionToolHeadlines.SupplementAllOtherPublic,
                        supplementRows);
                }

                if (selectedSupplementIdSet.Count > 0)
                {
                    selectedSupplementRows = DrugInteractionToolReportMapping.BuildSubsection(
                        "specific_drug.supplements.selected",
                        DrugInteractionToolHeadlines.SupplementSelected,
                        SliceSupplementRows(
                            selection,
                            context,
                            bulk,
                            takenNowSupplementIds: null,
                            includeAllCatalog: false,
                            scopeIds: selectedSupplementIdSet,
                            source: DuplicationDrugSource.Selected));
                }

                supplements = new PublicSupplementInteractionSectionReadModel(
                    SectionCode: "specific_drug.supplements",
                    AllOther: allOtherSupplements,
                    Selected: selectedSupplementRows);
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
                ManualDrugLimit: manualDrugCount,
                SelectedSupplementCount: selectedSupplements.Count,
                SelectedFoodCount: selectedFoods.Count,
                ShowPossibleInteractions: showPossibleInteractions),
            Info: ReportInfoSectionBuilder.Build(
                MapInfoDrugSelections(selections),
                includeUserData: false,
                regularSupplements: [],
                regularFoods: [],
                selectedSupplements: selectedSupplements,
                selectedFoods: selectedFoods,
                audience: ReportAudience.Public),
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
        IReadOnlyDictionary<Guid, IReadOnlyList<OwnSupplementSource>> ownSupplementSourcesBySupplementId,
        IReadOnlyCollection<ClientRegularSupplement> regularSupplements,
        IReadOnlyCollection<ClientRegularFood> regularFoods,
        IReadOnlyList<Supplement> selectedSupplements,
        IReadOnlyList<Food> selectedFoods,
        ReportAudience audience,
        bool showPossibleInteractions,
        AllergyIndexes allergyIndexes,
        CancellationToken cancellationToken)
    {
        ReportCompositionContext context = BuildCompositionContext(selections);
        bool includePossible = showAllFoodAndSupplementInteractions && showPossibleInteractions;

        HashSet<Guid> selectedSupplementIdSet = selectedSupplements.Select(item => item.Id).ToHashSet();
        HashSet<Guid> selectedFoodIdSet = selectedFoods.Select(item => item.Id).ToHashSet();

        HashSet<Guid> takenNowAndOwnMemberSupplementIds = takenNowSupplementIds
            .Concat(ownSupplementSourcesBySupplementId.Keys)
            .ToHashSet();

        // the bulk load is scoped to taken-now plus selected items, so selected items are analysed like taken-now ones
        // with food and supplement interactions off, nothing about foods or supplements is analysed at all
        HashSet<Guid> foodScopeIds = showAllFoodAndSupplementInteractions
            ? (includeUserData ? takenNowFoodIds : new HashSet<Guid>()).Concat(selectedFoodIdSet).ToHashSet()
            : new HashSet<Guid>();
        HashSet<Guid> supplementScopeIds = showAllFoodAndSupplementInteractions
            ? (includeUserData ? takenNowAndOwnMemberSupplementIds : new HashSet<Guid>())
                .Concat(selectedSupplementIdSet)
                .ToHashSet()
            : new HashSet<Guid>();

        BulkExpandedContext bulk = await BuildBulkExpandedContextAsync(
            context,
            includeAllFood: includePossible,
            takenNowFoodIds: foodScopeIds.Count > 0 ? foodScopeIds : null,
            includeAllSupplements: includePossible,
            takenNowSupplementIds: supplementScopeIds.Count > 0 ? supplementScopeIds : null,
            cancellationToken);

        Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> crossDrugPairs =
            BuildCrossDrugPairs(context, bulk);

        DrugDuplicationSectionReadModel duplicationSection = await BuildDuplicationSectionAsync(
            context,
            cancellationToken,
            includeUserData ? regularSupplements : null,
            selectedSupplements,
            includeSupplementDuplication: showAllFoodAndSupplementInteractions);
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
                ? DrugAllergyProblemIndexBuilder.SliceProblems(selection.Form.Id, allergyIndexes)
                : null;

            SpecificDrugAllergiesSectionReadModel? allergies = includeUserData
                ? DrugAllergyProblemIndexBuilder.SliceAllergies(selection.Form.Id, selection.Drug, allergyIndexes)
                : null;

            RegisteredFoodInteractionSectionReadModel? foods = SliceRegisteredFoodSection(
                selection,
                context,
                bulk,
                includeUserData,
                showOralGuidance,
                showAllFoodAndSupplementInteractions,
                takenNowFoodIds,
                mealTimes,
                selectedFoodIdSet,
                audience,
                includePossible);

            RegisteredSupplementInteractionSectionReadModel? supplements = SliceRegisteredSupplementSection(
                selection,
                context,
                bulk,
                includeUserData,
                showAllFoodAndSupplementInteractions,
                takenNowSupplementIds,
                takenNowAndOwnMemberSupplementIds,
                ownSupplementSourcesBySupplementId,
                selectedSupplementIdSet,
                audience,
                includePossible);

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
                ManualDrugLimit: manualDrugCount,
                SelectedSupplementCount: selectedSupplements.Count,
                SelectedFoodCount: selectedFoods.Count,
                Audience: audience,
                ShowPossibleInteractions: showPossibleInteractions),
            UserInfoSection: userInfoSection,
            Info: ReportInfoSectionBuilder.Build(
                MapInfoDrugSelections(selections),
                includeUserData,
                regularSupplements,
                regularFoods,
                selectedSupplements,
                selectedFoods,
                audience),
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
                selection.Form,
                selection.Source);
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
        bool includeAllFood,
        IReadOnlyCollection<Guid>? takenNowFoodIds,
        bool includeAllSupplements,
        IReadOnlyCollection<Guid>? takenNowSupplementIds,
        CancellationToken cancellationToken)
    {
        var allSubstanceIds = context.ActiveSubstanceNameById.Keys.ToHashSet();

        List<InteractionExpanded> allRows = await _interactionRepository.SearchExpandedRowsBulkAsync(
            allSubstanceIds,
            includeAllFood,
            takenNowFoodIds,
            includeAllSupplements,
            takenNowSupplementIds,
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
            FoodRowsBySubstanceId = foodRows.ToLookup(row => row.PartyIdA),
            SupplementRowsBySubstanceId = supplementRows.ToLookup(row => row.PartyIdA),
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
        List<DrugFormIntake> intakes = context.Selections
            .Select(selection => new DrugFormIntake(selection.Drug, selection.Form))
            .ToList();

        return DrugPairInteractionBuilder.BuildCrossDrugPairs(
            intakes,
            bulk.DrugDrugRows,
            bulk.InteractionById,
            context.ActiveSubstanceNameById);
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
        CancellationToken cancellationToken,
        IReadOnlyCollection<ClientRegularSupplement>? regularSupplements = null,
        IReadOnlyCollection<Supplement>? selectedSupplements = null,
        bool includeSupplementDuplication = true)
    {
        List<DuplicationSelection> selections = context.Selections
            .Select(selection => new DuplicationSelection(selection.Drug, selection.Form, selection.Source))
            .ToList();

        List<ActiveSubstanceGroup> groups = DrugDuplicationBuilder.HasMultipleDrugs(selections)
            ? await _activeSubstanceGroupRepository.ListByMemberIdsAsync(
                context.ActiveSubstanceNameById.Keys.ToHashSet(),
                cancellationToken)
            : new List<ActiveSubstanceGroup>();

        return DrugDuplicationBuilder.Build(
            selections,
            context.ActiveSubstanceNameById,
            groups,
            regularSupplements,
            selectedSupplements,
            includeSupplementDuplication);
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

            string? overrideText = substance.AlcoholInteractionOverride?.AlcoholSpecialText;
            string? specialText = string.IsNullOrWhiteSpace(overrideText) ? null : overrideText;

            result[substanceId] = new AlcoholInteractionRowReadModel(
                ActiveSubstanceName: substance.ActiveSubstanceName,
                DefaultText: defaultAlcoholText,
                SpecialText: specialText);
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
        IReadOnlyCollection<ClientMealTime> mealTimes,
        HashSet<Guid> selectedFoodIds,
        ReportAudience audience,
        bool includePossible)
    {
        bool showTakenNow = includeUserData && showAllFoodAndSupplementInteractions;
        bool showAllOther = includePossible;
        bool showSelected = showAllFoodAndSupplementInteractions && selectedFoodIds.Count > 0;
        HashSet<Guid>? regularFoodIds = includeUserData ? takenNowFoodIds : null;

        if (!showTakenNow && !showAllOther && !showSelected && !(includeUserData && showOralGuidance && selection.Form.IsOral))
        {
            return null;
        }

        OralGuidanceSubsectionReadModel? oralGuidance = includeUserData && showOralGuidance && selection.Form.IsOral
            ? BuildOralGuidanceSubsection(selection, context, mealTimes)
            : null;

        ReportSubsectionReadModel<FoodInteractionRowReadModel>? takenNow = null;
        ReportSubsectionReadModel<FoodInteractionRowReadModel>? allOther = null;
        ReportSubsectionReadModel<FoodInteractionRowReadModel>? selected = null;

        if (showTakenNow)
        {
            List<FoodInteractionRowReadModel> takenRows = SliceFoodRows(
                selection,
                context,
                bulk,
                takenNowFoodIds,
                includeAllCatalog: false,
                source: DuplicationDrugSource.TakingNow);

            takenNow = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.foods.taken_now",
                DrugInteractionToolHeadlines.FoodTakenNow,
                takenRows);
        }

        if (showSelected)
        {
            List<FoodInteractionRowReadModel> selectedRows = SliceFoodRows(
                selection,
                context,
                bulk,
                regularFoodIds,
                includeAllCatalog: false,
                scopeIds: selectedFoodIds,
                source: DuplicationDrugSource.Selected);

            selected = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.foods.selected",
                audience == ReportAudience.Provider
                    ? DrugInteractionToolHeadlines.FoodSelectedPrescription
                    : DrugInteractionToolHeadlines.FoodSelected,
                selectedRows);
        }

        if (showAllOther)
        {
            HashSet<Guid>? excludedIds = showSelected
                ? (regularFoodIds ?? new HashSet<Guid>()).Concat(selectedFoodIds).ToHashSet()
                : null;

            List<FoodInteractionRowReadModel> otherRows = SliceFoodRows(
                selection,
                context,
                bulk,
                regularFoodIds,
                includeAllCatalog: true,
                scopeIds: excludedIds);

            allOther = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.foods.all_other",
                DrugInteractionToolHeadlines.FoodAllOtherRegistered,
                otherRows);
        }

        if (oralGuidance is null && takenNow is null && allOther is null && selected is null)
        {
            return null;
        }

        return new RegisteredFoodInteractionSectionReadModel(
            SectionCode: "specific_drug.foods",
            OralGuidance: oralGuidance,
            TakenNow: takenNow,
            AllOther: allOther,
            Selected: selected);
    }

    private static RegisteredSupplementInteractionSectionReadModel? SliceRegisteredSupplementSection(
        DrugFormSelection selection,
        ReportCompositionContext context,
        BulkExpandedContext bulk,
        bool includeUserData,
        bool showAllFoodAndSupplementInteractions,
        HashSet<Guid> takenNowSupplementIds,
        HashSet<Guid> takenNowAndOwnMemberSupplementIds,
        IReadOnlyDictionary<Guid, IReadOnlyList<OwnSupplementSource>> ownSupplementSourcesBySupplementId,
        HashSet<Guid> selectedSupplementIds,
        ReportAudience audience,
        bool includePossible)
    {
        if (!showAllFoodAndSupplementInteractions)
        {
            return null;
        }

        bool showTakenNow = includeUserData;
        bool showAllOther = includePossible;
        bool showSelected = selectedSupplementIds.Count > 0;

        if (!showTakenNow && !showAllOther && !showSelected)
        {
            return null;
        }

        ReportSubsectionReadModel<SupplementInteractionRowReadModel>? takenNow = null;
        ReportSubsectionReadModel<SupplementInteractionRowReadModel>? allOther = null;
        ReportSubsectionReadModel<SupplementInteractionRowReadModel>? selected = null;
        ReportSubsectionReadModel<OwnSupplementInteractionRowReadModel>? ownSupplement = null;

        if (showTakenNow)
        {
            List<SupplementInteractionRowReadModel> takenRows = SliceSupplementRows(
                selection,
                context,
                bulk,
                takenNowSupplementIds,
                includeAllCatalog: false,
                source: DuplicationDrugSource.TakingNow);

            takenNow = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.supplements.taken_now",
                DrugInteractionToolHeadlines.SupplementTakenNow,
                takenRows);

            List<OwnSupplementInteractionRowReadModel> ownSupplementRows = SliceOwnSupplementRows(
                selection,
                context,
                bulk,
                ownSupplementSourcesBySupplementId);

            ownSupplement = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.supplements.own_supplement",
                DrugInteractionToolHeadlines.SupplementOwnSupplement,
                ownSupplementRows);
        }

        if (showSelected)
        {
            List<SupplementInteractionRowReadModel> selectedRows = SliceSupplementRows(
                selection,
                context,
                bulk,
                includeUserData ? takenNowSupplementIds : null,
                includeAllCatalog: false,
                scopeIds: selectedSupplementIds,
                source: DuplicationDrugSource.Selected);

            selected = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.supplements.selected",
                audience == ReportAudience.Provider
                    ? DrugInteractionToolHeadlines.SupplementSelectedPrescription
                    : DrugInteractionToolHeadlines.SupplementSelected,
                selectedRows);
        }

        if (showAllOther)
        {
            HashSet<Guid>? regularIds = includeUserData ? takenNowAndOwnMemberSupplementIds : null;
            HashSet<Guid>? excludedIds = showSelected
                ? (regularIds ?? new HashSet<Guid>()).Concat(selectedSupplementIds).ToHashSet()
                : null;

            List<SupplementInteractionRowReadModel> otherRows = SliceSupplementRows(
                selection,
                context,
                bulk,
                regularIds,
                includeAllCatalog: true,
                scopeIds: excludedIds);

            allOther = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.supplements.all_other",
                DrugInteractionToolHeadlines.SupplementAllOtherRegistered,
                otherRows);
        }

        return new RegisteredSupplementInteractionSectionReadModel(
            SectionCode: "specific_drug.supplements",
            TakenNow: takenNow,
            AllOther: allOther,
            OwnSupplement: ownSupplement,
            Selected: selected);
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

        string? drugIntakeTime = selection.RegularIntake is { IntakeTimes.Count: > 0 } regularIntake
            ? string.Join(", ", regularIntake.IntakeTimes)
            : null;

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
            DrugIntakeTime: drugIntakeTime,
            ActiveSubstanceGuidanceRows: guidanceRows);
    }

    private static List<FoodInteractionRowReadModel> SliceFoodRows(
        DrugFormSelection selection,
        ReportCompositionContext context,
        BulkExpandedContext bulk,
        HashSet<Guid>? takenNowFoodIds,
        bool includeAllCatalog,
        HashSet<Guid>? scopeIds = null,
        DuplicationDrugSource? source = null)
    {
        var substanceIds = selection.Drug.ActiveSubstances.Select(substance => substance.Id).ToHashSet();
        if (substanceIds.Count == 0)
        {
            return new List<FoodInteractionRowReadModel>();
        }

        // scopeIds restricts (includeAllCatalog false) or excludes (includeAllCatalog true) rows; the taken-now set only flags rows
        HashSet<Guid>? filterIds = scopeIds ?? takenNowFoodIds;

        var relevantRows = substanceIds.SelectMany(substanceId => bulk.FoodRowsBySubstanceId[substanceId]);

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
            IReadOnlyCollection<string> aliasNames;

            if (foodParty?.PartyType == InteractionType.FoodGroup && foodParty.FoodGroupId.HasValue
                && bulk.FoodGroupById.TryGetValue(foodParty.FoodGroupId.Value, out FoodGroup? group))
            {
                foodId = group.Id;
                foodName = group.Name;
                aliasNames = Array.Empty<string>();
            }
            else if (bulk.FoodById.TryGetValue(expanded.PartyIdB, out Food? food))
            {
                foodId = food.Id;
                foodName = food.Name;
                aliasNames = food.Aliases.ToList();
            }
            else
            {
                continue;
            }

            bool isTakenNow = takenNowFoodIds?.Contains(expanded.PartyIdB) == true
                || takenNowFoodIds?.Contains(foodId) == true;

            bool isInFilter = filterIds?.Contains(expanded.PartyIdB) == true
                || filterIds?.Contains(foodId) == true;

            if (!includeAllCatalog && filterIds is not null && !isInFilter)
            {
                continue;
            }

            if (includeAllCatalog && filterIds is not null && isInFilter && filterIds.Count > 0)
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
                AliasNames: aliasNames,
                InteractionId: interaction.Id,
                DefaultText: defaultText,
                StomachText: stomachText,
                Severity: severity,
                IsTakenNow: isTakenNow,
                DisplayLine: DrugInteractionToolReportMapping.BuildFoodOrSupplementDisplayLine(
                    substanceName,
                    DrugInteractionToolReportMapping.BuildNameWithAliases(foodName, aliasNames),
                    isTakenNow,
                    defaultText,
                    stomachText,
                    severity),
                Source: source));
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
        bool includeAllCatalog,
        HashSet<Guid>? scopeIds = null,
        DuplicationDrugSource? source = null)
    {
        var substanceIds = selection.Drug.ActiveSubstances.Select(substance => substance.Id).ToHashSet();
        if (substanceIds.Count == 0)
        {
            return new List<SupplementInteractionRowReadModel>();
        }

        // scopeIds restricts (includeAllCatalog false) or excludes (includeAllCatalog true) rows; the taken-now set only flags rows
        HashSet<Guid>? filterIds = scopeIds ?? takenNowSupplementIds;

        var relevantRows = substanceIds.SelectMany(substanceId => bulk.SupplementRowsBySubstanceId[substanceId]);

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
            IReadOnlyCollection<string> aliasNames;

            if (supplementParty?.PartyType == InteractionType.SupplementGroup
                && supplementParty.SupplementGroupId.HasValue
                && bulk.SupplementGroupById.TryGetValue(supplementParty.SupplementGroupId.Value, out SupplementGroup? group))
            {
                supplementId = group.Id;
                supplementName = group.Name ?? string.Empty;
                aliasNames = Array.Empty<string>();
            }
            else if (bulk.SupplementById.TryGetValue(expanded.PartyIdB, out Supplement? supplement))
            {
                supplementId = supplement.Id;
                supplementName = supplement.Name ?? string.Empty;
                aliasNames = supplement.Aliases.ToList();
            }
            else
            {
                continue;
            }

            bool isTakenNow = takenNowSupplementIds?.Contains(expanded.PartyIdB) == true
                || takenNowSupplementIds?.Contains(supplementId) == true;

            bool isInFilter = filterIds?.Contains(expanded.PartyIdB) == true
                || filterIds?.Contains(supplementId) == true;

            if (!includeAllCatalog && filterIds is not null && !isInFilter)
            {
                continue;
            }

            if (includeAllCatalog && filterIds is not null && isInFilter && filterIds.Count > 0)
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
                AliasNames: aliasNames,
                InteractionId: interaction.Id,
                DefaultText: defaultText,
                StomachText: stomachText,
                Severity: severity,
                IsTakenNow: isTakenNow,
                DisplayLine: DrugInteractionToolReportMapping.BuildFoodOrSupplementDisplayLine(
                    substanceName,
                    DrugInteractionToolReportMapping.BuildNameWithAliases(supplementName, aliasNames),
                    isTakenNow,
                    defaultText,
                    stomachText,
                    severity),
                Source: source));
        }

        return rows
            .GroupBy(row => (row.ActiveSubstanceId, row.SupplementId, row.InteractionId))
            .Select(group => group.First())
            .OrderBy(row => row.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.SupplementName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<OwnSupplementInteractionRowReadModel> SliceOwnSupplementRows(
        DrugFormSelection selection,
        ReportCompositionContext context,
        BulkExpandedContext bulk,
        IReadOnlyDictionary<Guid, IReadOnlyList<OwnSupplementSource>> ownSupplementSourcesBySupplementId)
    {
        if (ownSupplementSourcesBySupplementId.Count == 0)
        {
            return new List<OwnSupplementInteractionRowReadModel>();
        }

        var substanceIds = selection.Drug.ActiveSubstances.Select(substance => substance.Id).ToHashSet();
        var relevantRows = substanceIds.SelectMany(substanceId => bulk.SupplementRowsBySubstanceId[substanceId]);

        List<OwnSupplementInteractionRowReadModel> rows = new();

        foreach (InteractionExpanded expanded in relevantRows.OrderBy(row => row.PartyIdA).ThenBy(row => row.PartyIdB))
        {
            bool isOwnSupplementMember = ownSupplementSourcesBySupplementId.TryGetValue(
                expanded.PartyIdB,
                out IReadOnlyList<OwnSupplementSource>? ownSupplementSources);

            if (!isOwnSupplementMember
                || !bulk.InteractionById.TryGetValue(expanded.InteractionId, out Interaction? interaction)
                || !bulk.SupplementById.TryGetValue(expanded.PartyIdB, out Supplement? memberSupplement))
            {
                continue;
            }

            string substanceName = context.ActiveSubstanceNameById[expanded.PartyIdA];
            string supplementName = memberSupplement.Name ?? string.Empty;
            IReadOnlyCollection<string> aliasNames = memberSupplement.Aliases.ToList();
            string? stomachText = selection.Form.IsOral ? interaction.StomachText : null;
            string defaultText = interaction.DefaultText ?? string.Empty;
            string severity = interaction.InteractionSeverity.Name;

            foreach (OwnSupplementSource ownSupplementSource in ownSupplementSources!)
            {
                rows.Add(new OwnSupplementInteractionRowReadModel(
                    ActiveSubstanceId: expanded.PartyIdA,
                    ActiveSubstanceName: substanceName,
                    OwnSupplementId: ownSupplementSource.OwnSupplementId,
                    OwnSupplementName: ownSupplementSource.OwnSupplementName,
                    SupplementId: memberSupplement.Id,
                    SupplementName: supplementName,
                    AliasNames: aliasNames,
                    InteractionId: interaction.Id,
                    DefaultText: defaultText,
                    StomachText: stomachText,
                    Severity: severity,
                    IsTakenNow: true,
                    DisplayLine: DrugInteractionToolReportMapping.BuildOwnSupplementDisplayLine(
                        substanceName,
                        DrugInteractionToolReportMapping.BuildNameWithAliases(supplementName, aliasNames),
                        ownSupplementSource.OwnSupplementName,
                        defaultText,
                        stomachText,
                        severity),
                    Source: DuplicationDrugSource.TakingNow));
            }
        }

        return rows
            .GroupBy(row => (row.ActiveSubstanceId, row.OwnSupplementId, row.SupplementId, row.InteractionId))
            .Select(group => group.First())
            .OrderBy(row => row.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.OwnSupplementName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.SupplementName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<InfoDrugSelection> MapInfoDrugSelections(IReadOnlyList<DrugFormSelection> selections) =>
        selections
            .Select(selection => new InfoDrugSelection(
                selection.Drug,
                selection.Form,
                selection.Source,
                selection.RegularIntake))
            .ToList();

    private static List<DrugFormIntake> BuildPlannedDrugIntakes(
        IReadOnlyList<DrugFormSelection> manualSelections,
        IReadOnlyCollection<ClientRegularDrug> regularDrugs)
    {
        HashSet<Guid> takenNowFormIds = regularDrugs
            .Where(row =>
                row.Status == RegularDrugStatus.Active
                && row.Drug is not null
                && row.DrugForm is not null
                && RegularDrugEligibility.IsEligibleDrug(row.Drug))
            .Select(row => row.DrugForm.Id)
            .ToHashSet();

        return manualSelections
            .Where(selection => !takenNowFormIds.Contains(selection.Form.Id))
            .Select(selection => new DrugFormIntake(selection.Drug, selection.Form))
            .ToList();
    }

    private async Task<UserMedicalBundle> LoadUserMedicalBundleAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<UserHealthCondition> healthConditions =
            await _userHealthConditionRepository.ListAllByUserIdAsync(userId, cancellationToken);
        List<DrugProblem> drugProblems =
            await _drugProblemRepository.ListAllByUserIdAsync(userId, cancellationToken);
        List<ClientDrugAllergy> drugAllergies =
            await _clientDrugAllergyRepository.ListAllByUserIdAsync(userId, cancellationToken);
        List<ClientFoodAllergy> foodAllergies =
            await _clientFoodAllergyRepository.ListAllByUserIdAsync(userId, cancellationToken);
        List<ClientSupplementAllergy> supplementAllergies =
            await _clientSupplementAllergyRepository.ListAllByUserIdAsync(userId, cancellationToken);
        List<ClientOtherAllergy> otherAllergies =
            await _clientOtherAllergyRepository.ListAllByUserIdAsync(userId, cancellationToken);
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
}
