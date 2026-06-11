using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
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
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.DrugInteractionToolFeature.ApplicationServices;

/// <summary>
/// Orchestrates drug interaction tool report composition for public and registered audiences [description].
/// </summary>
public sealed class DrugInteractionToolReportApplicationService(
    IDrugRepository _drugRepository,
    IInteractionRepository _interactionRepository,
    IActiveSubstanceRepository _activeSubstanceRepository,
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
    IClientReproductiveHealthProfileRepository _clientReproductiveHealthProfileRepository) : IDrugInteractionToolReportApplicationService
{
    private const int PageSize = 40;
    private const string AlcoholDefaultTextKey = "activeSubstance.alcohol.defaultText";

    private sealed record DrugFormSelection(
        Drug Drug,
        DrugForm Form,
        SelectedDrugSource? Source);

    private sealed record CrossDrugPairKey(Guid LeftFormId, Guid RightFormId);

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
            showAllDrugsInteractions,
            showOralGuidance,
            showAllFoodAndSupplementInteractions,
            cancellationToken);
    }

    public async Task<ErrorOr<RegisteredDrugInteractionToolReportReadModel>> ComputeRegisteredReportAsync(
        Guid userId,
        IReadOnlyCollection<Guid> manualDrugFormIds,
        bool includeUserData,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        CancellationToken cancellationToken)
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
        List<ClientDrugAllergy> userDrugAllergies = new();
        List<DrugProblem> userDrugProblems = new();

        if (includeUserData)
        {
            effectiveSelections = await MergeRegularDrugsAsync(userId, effectiveSelections, cancellationToken);
            userInfoSection = await BuildUserInfoSectionAsync(userId, cancellationToken);
            takenNowFoodIds = await LoadTakenNowFoodIdsAsync(userId, cancellationToken);
            takenNowSupplementIds = await LoadTakenNowSupplementIdsAsync(userId, cancellationToken);
            userDrugAllergies = await LoadAllDrugAllergiesAsync(userId, cancellationToken);
            userDrugProblems = await LoadAllDrugProblemsAsync(userId, cancellationToken);
        }

        return await BuildRegisteredReportAsync(
            userId,
            effectiveSelections,
            includeUserData,
            showAllDrugsInteractions,
            showOralGuidance,
            showAllFoodAndSupplementInteractions,
            userInfoSection,
            takenNowFoodIds,
            takenNowSupplementIds,
            userDrugAllergies,
            userDrugProblems,
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

    private async Task<List<DrugFormSelection>> MergeRegularDrugsAsync(
        Guid userId,
        List<DrugFormSelection> manualSelections,
        CancellationToken cancellationToken)
    {
        var byFormId = manualSelections.ToDictionary(selection => selection.Form.Id);
        List<ClientRegularDrug> regularRows = await LoadAllLiveRegularDrugsAsync(userId, cancellationToken);

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
                    Source = SelectedDrugSource.ManualAndUserRegular
                };
                continue;
            }

            byFormId[regularRow.DrugFormId] = new DrugFormSelection(
                regularRow.Drug,
                regularRow.DrugForm,
                SelectedDrugSource.UserRegular);
        }

        return byFormId.Values
            .OrderBy(selection => selection.Drug.CommercialName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(selection => selection.Form.Id)
            .ToList();
    }

    private async Task<ErrorOr<PublicDrugInteractionToolReportReadModel>> BuildPublicReportAsync(
        IReadOnlyList<DrugFormSelection> selections,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        CancellationToken cancellationToken)
    {
        ReportCompositionContext context = await BuildCompositionContextAsync(
            selections,
            showOralGuidance,
            cancellationToken);

        Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> crossDrugPairs =
            await BuildCrossDrugPairRowsAsync(context, cancellationToken);

        DrugDuplicationSectionReadModel duplicationSection = await BuildDuplicationSectionAsync(context, cancellationToken);
        List<PublicSpecificDrugReportReadModel> specificReports = new();

        foreach (DrugFormSelection selection in selections)
        {
            SelectedDrugSummaryReadModel drugSummary = context.SummariesByFormId[selection.Form.Id];
            AlcoholInteractionSectionReadModel alcohol = await BuildAlcoholSectionAsync(
                selection,
                context.ActiveSubstanceById,
                cancellationToken);

            DrugInteractionSectionReadModel drugsSection = BuildPerDrugDrugSection(
                selection.Form.Id,
                context,
                crossDrugPairs);

            PublicFoodInteractionSectionReadModel? foods = null;
            PublicSupplementInteractionSectionReadModel? supplements = null;

            if (showAllFoodAndSupplementInteractions)
            {
                foods = await BuildPublicFoodSectionAsync(
                    selection,
                    context,
                    takenNowFoodIds: null,
                    includeAllCatalog: true,
                    cancellationToken);

                supplements = await BuildPublicSupplementSectionAsync(
                    selection,
                    context,
                    takenNowSupplementIds: null,
                    includeAllCatalog: true,
                    cancellationToken);
            }

            specificReports.Add(new PublicSpecificDrugReportReadModel(
                Drug: drugSummary,
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
                ManualDrugLimit: 3),
            SelectedDrugs: selections.Select(selection => context.SummariesByFormId[selection.Form.Id]).ToList(),
            SpecificDrugReports: specificReports,
            AllDrugsInteractionsSection: globalSection,
            DrugDuplicationSection: duplicationSection);
    }

    private async Task<ErrorOr<RegisteredDrugInteractionToolReportReadModel>> BuildRegisteredReportAsync(
        Guid userId,
        IReadOnlyList<DrugFormSelection> selections,
        bool includeUserData,
        bool showAllDrugsInteractions,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        UserInfoSectionReadModel? userInfoSection,
        HashSet<Guid> takenNowFoodIds,
        HashSet<Guid> takenNowSupplementIds,
        List<ClientDrugAllergy> userDrugAllergies,
        List<DrugProblem> userDrugProblems,
        CancellationToken cancellationToken)
    {
        ReportCompositionContext context = await BuildCompositionContextAsync(
            selections,
            showOralGuidance,
            cancellationToken);

        Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> crossDrugPairs =
            await BuildCrossDrugPairRowsAsync(context, cancellationToken);

        DrugDuplicationSectionReadModel duplicationSection = await BuildDuplicationSectionAsync(context, cancellationToken);
        List<ClientMealTime> mealTimes = includeUserData
            ? await _clientMealTimeRepository.ListByUserAsync(userId, cancellationToken)
            : new List<ClientMealTime>();

        List<RegisteredSpecificDrugReportReadModel> specificReports = new();

        foreach (DrugFormSelection selection in selections)
        {
            SelectedDrugSummaryReadModel drugSummary = context.SummariesByFormId[selection.Form.Id];
            AlcoholInteractionSectionReadModel alcohol = await BuildAlcoholSectionAsync(
                selection,
                context.ActiveSubstanceById,
                cancellationToken);

            DrugInteractionSectionReadModel drugsSection = BuildPerDrugDrugSection(
                selection.Form.Id,
                context,
                crossDrugPairs);

            SpecificDrugProblemsSectionReadModel? problems = includeUserData
                ? BuildSpecificDrugProblems(selection.Drug.Id, userDrugProblems)
                : null;

            SpecificDrugAllergiesSectionReadModel? allergies = includeUserData
                ? BuildSpecificDrugAllergies(selection, userDrugAllergies, context)
                : null;

            RegisteredFoodInteractionSectionReadModel? foods = await BuildRegisteredFoodSectionAsync(
                selection,
                context,
                includeUserData,
                showOralGuidance,
                showAllFoodAndSupplementInteractions,
                takenNowFoodIds,
                mealTimes,
                cancellationToken);

            RegisteredSupplementInteractionSectionReadModel? supplements = await BuildRegisteredSupplementSectionAsync(
                selection,
                context,
                includeUserData,
                showAllFoodAndSupplementInteractions,
                takenNowSupplementIds,
                cancellationToken);

            specificReports.Add(new RegisteredSpecificDrugReportReadModel(
                Drug: drugSummary,
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
                ManualDrugLimit: 20),
            UserInfoSection: userInfoSection,
            SelectedDrugs: selections.Select(selection => context.SummariesByFormId[selection.Form.Id]).ToList(),
            SpecificDrugReports: specificReports,
            AllDrugsInteractionsSection: globalSection,
            DrugDuplicationSection: duplicationSection);
    }

    private sealed class ReportCompositionContext
    {
        public required IReadOnlyList<DrugFormSelection> Selections { get; init; }

        public required Dictionary<Guid, SelectedDrugSummaryReadModel> SummariesByFormId { get; init; }

        public required Dictionary<Guid, ActiveSubstance> ActiveSubstanceById { get; init; }

        public required Dictionary<Guid, string> ActiveSubstanceNameById { get; init; }
    }

    private async Task<ReportCompositionContext> BuildCompositionContextAsync(
        IReadOnlyList<DrugFormSelection> selections,
        bool showOralGuidance,
        CancellationToken cancellationToken)
    {
        var substanceIds = selections
            .SelectMany(selection => selection.Drug.ActiveSubstances.Select(substance => substance.Id))
            .ToHashSet();

        IReadOnlyCollection<ActiveSubstance> loadedSubstances = await _activeSubstanceRepository.GetByIdsAsync(
            substanceIds,
            cancellationToken);

        var activeSubstanceById = loadedSubstances.ToDictionary(substance => substance.Id);
        Dictionary<Guid, SelectedDrugSummaryReadModel> summariesByFormId = new();

        foreach (DrugFormSelection selection in selections)
        {
            summariesByFormId[selection.Form.Id] = DrugInteractionToolReportMapping.MapSelectedDrugSummary(
                selection.Drug,
                selection.Form,
                showOralGuidance,
                activeSubstanceById,
                selection.Source);
        }

        var substanceNameById = selections
            .SelectMany(selection => selection.Drug.ActiveSubstances)
            .GroupBy(substance => substance.Id)
            .ToDictionary(group => group.Key, group => group.First().ActiveSubstanceName);

        return new ReportCompositionContext
        {
            Selections = selections,
            SummariesByFormId = summariesByFormId,
            ActiveSubstanceById = activeSubstanceById,
            ActiveSubstanceNameById = substanceNameById
        };
    }

    private async Task<Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>>> BuildCrossDrugPairRowsAsync(
        ReportCompositionContext context,
        CancellationToken cancellationToken)
    {
        Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> result = new();
        var selections = context.Selections.ToList();

        for (int leftIndex = 0; leftIndex < selections.Count; leftIndex++)
        {
            for (int rightIndex = leftIndex + 1; rightIndex < selections.Count; rightIndex++)
            {
                DrugFormSelection left = selections[leftIndex];
                DrugFormSelection right = selections[rightIndex];
                CrossDrugPairKey forwardKey = new(left.Form.Id, right.Form.Id);
                CrossDrugPairKey reverseKey = new(right.Form.Id, left.Form.Id);

                List<SubstancePairInteractionRowReadModel> forwardRows = await BuildSubstancePairRowsBetweenDrugsAsync(
                    left,
                    right,
                    context,
                    cancellationToken);

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

                result[forwardKey] = forwardRows;
                result[reverseKey] = reverseRows;
            }
        }

        return result;
    }

    private async Task<List<SubstancePairInteractionRowReadModel>> BuildSubstancePairRowsBetweenDrugsAsync(
        DrugFormSelection current,
        DrugFormSelection compared,
        ReportCompositionContext context,
        CancellationToken cancellationToken)
    {
        var currentIds = current.Drug.ActiveSubstances.Select(substance => substance.Id).ToList();
        var comparedIds = compared.Drug.ActiveSubstances.Select(substance => substance.Id).ToList();

        List<InteractionExpanded> expandedRows = await _interactionRepository.SearchExpandedRowsByAtomicIdsAsync(
            ShortInteractionType.Substance,
            currentIds,
            ShortInteractionType.Substance,
            comparedIds,
            cancellationToken);

        List<InteractionExpanded> reverseExpandedRows = await _interactionRepository.SearchExpandedRowsByAtomicIdsAsync(
            ShortInteractionType.Substance,
            comparedIds,
            ShortInteractionType.Substance,
            currentIds,
            cancellationToken);

        var allExpanded = expandedRows
            .Concat(reverseExpandedRows)
            .GroupBy(row => row.Id)
            .Select(group => group.First())
            .ToList();

        return await MapExpandedSubstanceRowsAsync(
            allExpanded,
            currentIds.ToHashSet(),
            comparedIds.ToHashSet(),
            current.Form.IsOral && compared.Form.IsOral,
            context.ActiveSubstanceNameById,
            cancellationToken);
    }

    private async Task<List<SubstancePairInteractionRowReadModel>> MapExpandedSubstanceRowsAsync(
        IReadOnlyCollection<InteractionExpanded> expandedRows,
        HashSet<Guid> currentSubstanceIds,
        HashSet<Guid> comparedSubstanceIds,
        bool includeStomachText,
        IReadOnlyDictionary<Guid, string> substanceNameById,
        CancellationToken cancellationToken)
    {
        if (expandedRows.Count == 0)
        {
            return new List<SubstancePairInteractionRowReadModel>();
        }

        var interactionIds = expandedRows.Select(row => row.InteractionId).ToHashSet();
        List<Interaction> interactions = await _interactionRepository.GetByIdsAsync(interactionIds, cancellationToken);
        var interactionById = interactions.ToDictionary(interaction => interaction.Id);

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

            SelectedDrugSummaryReadModel currentSummary = context.SummariesByFormId[currentFormId];
            SelectedDrugSummaryReadModel comparedSummary = context.SummariesByFormId[other.Form.Id];

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
                .OrderBy(group => group.ComparedDrug.CommercialName, StringComparer.OrdinalIgnoreCase)
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

                SelectedDrugSummaryReadModel leftSummary = context.SummariesByFormId[left.Form.Id];
                SelectedDrugSummaryReadModel rightSummary = context.SummariesByFormId[right.Form.Id];

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
                .OrderBy(group => group.LeftDrug.CommercialName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(group => group.RightDrug.CommercialName, StringComparer.OrdinalIgnoreCase)
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
        Dictionary<Guid, List<SelectedDrugSummaryReadModel>> drugsBySubstance = new();

        foreach (DrugFormSelection selection in context.Selections)
        {
            SelectedDrugSummaryReadModel summary = context.SummariesByFormId[selection.Form.Id];
            foreach (ActiveSubstance substance in selection.Drug.ActiveSubstances)
            {
                if (!drugsBySubstance.TryGetValue(substance.Id, out List<SelectedDrugSummaryReadModel>? list))
                {
                    list = new List<SelectedDrugSummaryReadModel>();
                    drugsBySubstance[substance.Id] = list;
                }

                list.Add(summary);
            }
        }

        List<ExactDuplicationRowReadModel> rows = new();

        foreach ((Guid substanceId, List<SelectedDrugSummaryReadModel> affected) in drugsBySubstance
                     .Where(pair => pair.Value.Select(drug => drug.DrugFormId).Distinct().Count() > 1)
                     .OrderBy(pair => context.ActiveSubstanceNameById[pair.Key], StringComparer.OrdinalIgnoreCase))
        {
            string substanceName = context.ActiveSubstanceNameById[substanceId];
            IReadOnlyCollection<SelectedDrugSummaryReadModel> distinctDrugs = affected
                .GroupBy(drug => drug.DrugFormId)
                .Select(group => group.First())
                .OrderBy(drug => drug.CommercialName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(drug => drug.DrugFormId)
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
                .Select(selection => context.SummariesByFormId[selection.Form.Id])
                .GroupBy(summary => summary.DrugFormId)
                .Select(drugGroup => drugGroup.First())
                .OrderBy(summary => summary.CommercialName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(summary => summary.DrugFormId)
                .ToList();

            if (affectedDrugs.Count < 2)
            {
                continue;
            }

            bool onlyExactOverlap = affectedDrugs.All(drug =>
                drug.ActiveSubstances.All(sub =>
                    !memberIds.Contains(sub.ActiveSubstanceId) || exactSubstanceIds.Contains(sub.ActiveSubstanceId)));

            if (onlyExactOverlap && exactRows.Count > 0)
            {
                continue;
            }

            rows.Add(new GroupDuplicationRowReadModel(
                ActiveSubstanceGroupId: group.Id,
                GroupName: group.Name,
                AffectedDrugs: affectedDrugs,
                DisplayLine:
                $"Active substance group: {group.Name} IS SHARED BETWEEN MULTIPLE SELECTED DRUGS.",
                DuplicationKind: DuplicationKind.ActiveSubstanceGroup));
        }

        return rows;
    }

    private async Task<AlcoholInteractionSectionReadModel> BuildAlcoholSectionAsync(
        DrugFormSelection selection,
        IReadOnlyDictionary<Guid, ActiveSubstance> activeSubstanceById,
        CancellationToken cancellationToken)
    {
        string? defaultAlcoholText = (await _systemTextRepository.GetByKeyAsync(AlcoholDefaultTextKey, cancellationToken))?.Value;
        List<AlcoholInteractionRowReadModel> rows = new();

        foreach (ActiveSubstance substance in selection.Drug.ActiveSubstances
                     .OrderBy(s => s.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(s => s.Id))
        {
            if (!activeSubstanceById.TryGetValue(substance.Id, out ActiveSubstance? loaded) || !loaded.HasAlcoholInteraction)
            {
                continue;
            }

            string defaultText = loaded.AlcoholInteractionOverride?.AlcoholSpecialText
                ?? defaultAlcoholText
                ?? string.Empty;

            rows.Add(new AlcoholInteractionRowReadModel(
                ActiveSubstanceName: substance.ActiveSubstanceName,
                DefaultText: defaultText,
                SpecialText: loaded.AlcoholInteractionOverride?.AlcoholSpecialText));
        }

        return new AlcoholInteractionSectionReadModel(
            HeadlineText: DrugInteractionToolHeadlines.AlcoholSection,
            Rows: rows);
    }

    private async Task<PublicFoodInteractionSectionReadModel?> BuildPublicFoodSectionAsync(
        DrugFormSelection selection,
        ReportCompositionContext context,
        HashSet<Guid>? takenNowFoodIds,
        bool includeAllCatalog,
        CancellationToken cancellationToken)
    {
        List<FoodInteractionRowReadModel> allOtherRows = await BuildFoodInteractionRowsAsync(
            selection,
            context,
            takenNowFoodIds,
            includeAllCatalog,
            cancellationToken);

        if (allOtherRows.Count == 0 && takenNowFoodIds is null)
        {
            return new PublicFoodInteractionSectionReadModel(
                SectionCode: "specific_drug.foods",
                AllOther: DrugInteractionToolReportMapping.BuildSubsection(
                    "specific_drug.foods.all_other",
                    DrugInteractionToolHeadlines.FoodAllOtherPublic,
                    allOtherRows));
        }

        return new PublicFoodInteractionSectionReadModel(
            SectionCode: "specific_drug.foods",
            AllOther: DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.foods.all_other",
                DrugInteractionToolHeadlines.FoodAllOtherPublic,
                allOtherRows));
    }

    private async Task<PublicSupplementInteractionSectionReadModel?> BuildPublicSupplementSectionAsync(
        DrugFormSelection selection,
        ReportCompositionContext context,
        HashSet<Guid>? takenNowSupplementIds,
        bool includeAllCatalog,
        CancellationToken cancellationToken)
    {
        List<SupplementInteractionRowReadModel> allOtherRows = await BuildSupplementInteractionRowsAsync(
            selection,
            context,
            takenNowSupplementIds,
            includeAllCatalog,
            cancellationToken);

        return new PublicSupplementInteractionSectionReadModel(
            SectionCode: "specific_drug.supplements",
            AllOther: DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.supplements.all_other",
                DrugInteractionToolHeadlines.SupplementAllOtherPublic,
                allOtherRows));
    }

    private async Task<RegisteredFoodInteractionSectionReadModel?> BuildRegisteredFoodSectionAsync(
        DrugFormSelection selection,
        ReportCompositionContext context,
        bool includeUserData,
        bool showOralGuidance,
        bool showAllFoodAndSupplementInteractions,
        HashSet<Guid> takenNowFoodIds,
        IReadOnlyCollection<ClientMealTime> mealTimes,
        CancellationToken cancellationToken)
    {
        bool showTakenNow = includeUserData;
        bool showAllOther = includeUserData
            ? showAllFoodAndSupplementInteractions
            : showAllFoodAndSupplementInteractions;

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
            List<FoodInteractionRowReadModel> takenRows = await BuildFoodInteractionRowsAsync(
                selection,
                context,
                takenNowFoodIds,
                includeAllCatalog: false,
                cancellationToken);

            takenNow = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.foods.taken_now",
                DrugInteractionToolHeadlines.FoodTakenNow,
                takenRows);
        }

        if (showAllOther)
        {
            List<FoodInteractionRowReadModel> otherRows = await BuildFoodInteractionRowsAsync(
                selection,
                context,
                takenNowFoodIds: includeUserData ? takenNowFoodIds : null,
                includeAllCatalog: true,
                cancellationToken);

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

    private async Task<RegisteredSupplementInteractionSectionReadModel?> BuildRegisteredSupplementSectionAsync(
        DrugFormSelection selection,
        ReportCompositionContext context,
        bool includeUserData,
        bool showAllFoodAndSupplementInteractions,
        HashSet<Guid> takenNowSupplementIds,
        CancellationToken cancellationToken)
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
            List<SupplementInteractionRowReadModel> takenRows = await BuildSupplementInteractionRowsAsync(
                selection,
                context,
                takenNowSupplementIds,
                includeAllCatalog: false,
                cancellationToken);

            takenNow = DrugInteractionToolReportMapping.BuildSubsection(
                "specific_drug.supplements.taken_now",
                DrugInteractionToolHeadlines.SupplementTakenNow,
                takenRows);
        }

        if (showAllOther)
        {
            List<SupplementInteractionRowReadModel> otherRows = await BuildSupplementInteractionRowsAsync(
                selection,
                context,
                takenNowSupplementIds: includeUserData ? takenNowSupplementIds : null,
                includeAllCatalog: true,
                cancellationToken);

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

    private OralGuidanceSubsectionReadModel BuildOralGuidanceSubsection(
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
            .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .Select(substance =>
            {
                string guidance = context.ActiveSubstanceById.TryGetValue(substance.Id, out ActiveSubstance? loaded)
                    ? DrugInteractionToolReportMapping.FormatOralGuidanceText(loaded.OralMedicationIntakeGuidelines)
                    : string.Empty;

                return new ActiveSubstanceOralGuidanceRowReadModel(
                    ActiveSubstanceName: substance.ActiveSubstanceName,
                    GuidanceText: guidance,
                    DisplayLine: $"{substance.ActiveSubstanceName}: {guidance}");
            })
            .ToList();

        return new OralGuidanceSubsectionReadModel(
            HeadlineText: DrugInteractionToolHeadlines.OralGuidanceMealBlock,
            MealIntakeSummary: mealSummary,
            ActiveSubstanceGuidanceRows: guidanceRows);
    }

    private async Task<List<FoodInteractionRowReadModel>> BuildFoodInteractionRowsAsync(
        DrugFormSelection selection,
        ReportCompositionContext context,
        HashSet<Guid>? takenNowFoodIds,
        bool includeAllCatalog,
        CancellationToken cancellationToken)
    {
        var substanceIds = selection.Drug.ActiveSubstances.Select(substance => substance.Id).ToList();
        List<Guid> targetFoodIds = new();

        if (takenNowFoodIds is not null && takenNowFoodIds.Count > 0)
        {
            targetFoodIds.AddRange(takenNowFoodIds);
        }

        if (includeAllCatalog)
        {
            targetFoodIds.AddRange(await LoadAllFoodIdsAsync(cancellationToken));
        }

        targetFoodIds = targetFoodIds.Distinct().ToList();
        if (targetFoodIds.Count == 0 || substanceIds.Count == 0)
        {
            return new List<FoodInteractionRowReadModel>();
        }

        List<InteractionExpanded> expandedRows = await _interactionRepository.SearchExpandedRowsByAtomicIdsAsync(
            ShortInteractionType.Substance,
            substanceIds,
            ShortInteractionType.Food,
            targetFoodIds,
            cancellationToken);

        return await MapFoodRowsAsync(
            expandedRows,
            selection,
            context,
            takenNowFoodIds,
            includeAllCatalog,
            cancellationToken);
    }

    private async Task<List<SupplementInteractionRowReadModel>> BuildSupplementInteractionRowsAsync(
        DrugFormSelection selection,
        ReportCompositionContext context,
        HashSet<Guid>? takenNowSupplementIds,
        bool includeAllCatalog,
        CancellationToken cancellationToken)
    {
        var substanceIds = selection.Drug.ActiveSubstances.Select(substance => substance.Id).ToList();
        List<Guid> targetSupplementIds = new();

        if (takenNowSupplementIds is not null && takenNowSupplementIds.Count > 0)
        {
            targetSupplementIds.AddRange(takenNowSupplementIds);
        }

        if (includeAllCatalog)
        {
            targetSupplementIds.AddRange(await LoadAllSupplementIdsAsync(cancellationToken));
        }

        targetSupplementIds = targetSupplementIds.Distinct().ToList();
        if (targetSupplementIds.Count == 0 || substanceIds.Count == 0)
        {
            return new List<SupplementInteractionRowReadModel>();
        }

        List<InteractionExpanded> expandedRows = await _interactionRepository.SearchExpandedRowsByAtomicIdsAsync(
            ShortInteractionType.Substance,
            substanceIds,
            ShortInteractionType.Supplement,
            targetSupplementIds,
            cancellationToken);

        return await MapSupplementRowsAsync(
            expandedRows,
            selection,
            context,
            takenNowSupplementIds,
            includeAllCatalog,
            cancellationToken);
    }

    private async Task<List<FoodInteractionRowReadModel>> MapFoodRowsAsync(
        IReadOnlyCollection<InteractionExpanded> expandedRows,
        DrugFormSelection selection,
        ReportCompositionContext context,
        HashSet<Guid>? takenNowFoodIds,
        bool includeAllCatalog,
        CancellationToken cancellationToken)
    {
        if (expandedRows.Count == 0)
        {
            return new List<FoodInteractionRowReadModel>();
        }

        var interactions = (await _interactionRepository.GetByIdsAsync(
            expandedRows.Select(row => row.InteractionId).Distinct().ToList(),
            cancellationToken)).ToDictionary(interaction => interaction.Id);

        var foods = (await _foodRepository.GetByIdsAsync(
            expandedRows.Select(row => row.PartyIdB).Distinct().ToList(),
            cancellationToken)).ToDictionary(food => food.Id);

        var foodGroupIds = interactions.Values
            .SelectMany(interaction => interaction.Parties)
            .Where(party => party.PartyType == InteractionType.FoodGroup && party.FoodGroupId.HasValue)
            .Select(party => party.FoodGroupId!.Value)
            .Distinct()
            .ToList();

        var foodGroups = (await _foodGroupRepository.GetByIdsAsync(foodGroupIds, cancellationToken))
            .ToDictionary(group => group.Id);

        List<FoodInteractionRowReadModel> rows = new();

        foreach (InteractionExpanded expanded in expandedRows.OrderBy(row => row.PartyIdA).ThenBy(row => row.PartyIdB))
        {
            if (!interactions.TryGetValue(expanded.InteractionId, out Interaction? interaction))
            {
                continue;
            }

            InteractionParty? foodParty = interaction.Parties
                .FirstOrDefault(party => party.PartyType is InteractionType.Food or InteractionType.FoodGroup);

            InteractionPartyKind partyKind;
            Guid partyId;
            string displayName;

            if (foodParty?.PartyType == InteractionType.FoodGroup && foodParty.FoodGroupId.HasValue
                && foodGroups.TryGetValue(foodParty.FoodGroupId.Value, out FoodGroup? group))
            {
                partyKind = InteractionPartyKind.FoodGroup;
                partyId = group.Id;
                displayName = group.Name;
            }
            else if (foods.TryGetValue(expanded.PartyIdB, out Food? food))
            {
                partyKind = InteractionPartyKind.Food;
                partyId = food.Id;
                displayName = food.Name;
            }
            else
            {
                continue;
            }

            bool isTakenNow = takenNowFoodIds?.Contains(expanded.PartyIdB) == true
                || takenNowFoodIds?.Contains(partyId) == true;

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
                PartyKind: partyKind,
                PartyId: partyId,
                DisplayName: displayName,
                InteractionId: interaction.Id,
                DefaultText: defaultText,
                StomachText: stomachText,
                Severity: severity,
                IsTakenNow: isTakenNow,
                DisplayLine: DrugInteractionToolReportMapping.BuildFoodOrSupplementDisplayLine(
                    substanceName,
                    displayName,
                    isTakenNow,
                    defaultText,
                    stomachText,
                    severity)));
        }

        return rows
            .GroupBy(row => (row.ActiveSubstanceId, row.PartyKind, row.PartyId, row.InteractionId))
            .Select(group => group.First())
            .OrderBy(row => row.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<List<SupplementInteractionRowReadModel>> MapSupplementRowsAsync(
        IReadOnlyCollection<InteractionExpanded> expandedRows,
        DrugFormSelection selection,
        ReportCompositionContext context,
        HashSet<Guid>? takenNowSupplementIds,
        bool includeAllCatalog,
        CancellationToken cancellationToken)
    {
        if (expandedRows.Count == 0)
        {
            return new List<SupplementInteractionRowReadModel>();
        }

        var interactions = (await _interactionRepository.GetByIdsAsync(
            expandedRows.Select(row => row.InteractionId).Distinct().ToList(),
            cancellationToken)).ToDictionary(interaction => interaction.Id);

        var supplements = (await _supplementRepository.GetByIdsAsync(
            expandedRows.Select(row => row.PartyIdB).Distinct().ToList(),
            cancellationToken)).ToDictionary(supplement => supplement.Id);

        var supplementGroupIds = interactions.Values
            .SelectMany(interaction => interaction.Parties)
            .Where(party => party.PartyType == InteractionType.SupplementGroup && party.SupplementGroupId.HasValue)
            .Select(party => party.SupplementGroupId!.Value)
            .Distinct()
            .ToList();

        var supplementGroups = (await _supplementGroupRepository.GetByIdsAsync(
                supplementGroupIds,
                cancellationToken))
            .ToDictionary(group => group.Id);

        List<SupplementInteractionRowReadModel> rows = new();

        foreach (InteractionExpanded expanded in expandedRows.OrderBy(row => row.PartyIdA).ThenBy(row => row.PartyIdB))
        {
            if (!interactions.TryGetValue(expanded.InteractionId, out Interaction? interaction))
            {
                continue;
            }

            InteractionParty? supplementParty = interaction.Parties
                .FirstOrDefault(party => party.PartyType is InteractionType.Supplement or InteractionType.SupplementGroup);

            InteractionPartyKind partyKind;
            Guid partyId;
            string displayName;

            if (supplementParty?.PartyType == InteractionType.SupplementGroup
                && supplementParty.SupplementGroupId.HasValue
                && supplementGroups.TryGetValue(supplementParty.SupplementGroupId.Value, out SupplementGroup? group))
            {
                partyKind = InteractionPartyKind.SupplementGroup;
                partyId = group.Id;
                displayName = group.Name;
            }
            else if (supplements.TryGetValue(expanded.PartyIdB, out Supplement? supplement))
            {
                partyKind = InteractionPartyKind.Supplement;
                partyId = supplement.Id;
                displayName = supplement.Name;
            }
            else
            {
                continue;
            }

            bool isTakenNow = takenNowSupplementIds?.Contains(expanded.PartyIdB) == true
                || takenNowSupplementIds?.Contains(partyId) == true;

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
                PartyKind: partyKind,
                PartyId: partyId,
                DisplayName: displayName,
                InteractionId: interaction.Id,
                DefaultText: defaultText,
                StomachText: stomachText,
                Severity: severity,
                IsTakenNow: isTakenNow,
                DisplayLine: DrugInteractionToolReportMapping.BuildFoodOrSupplementDisplayLine(
                    substanceName,
                    displayName,
                    isTakenNow,
                    defaultText,
                    stomachText,
                    severity)));
        }

        return rows
            .GroupBy(row => (row.ActiveSubstanceId, row.PartyKind, row.PartyId, row.InteractionId))
            .Select(group => group.First())
            .OrderBy(row => row.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static SpecificDrugProblemsSectionReadModel? BuildSpecificDrugProblems(
        Guid drugId,
        IReadOnlyCollection<DrugProblem> problems)
    {
        var rows = problems
            .Where(problem => problem.Source == DrugProblemSource.Database && problem.DrugId == drugId)
            .Select(problem => new SpecificDrugProblemRowReadModel(
                ProblemDescription: problem.ProblemNote,
                HeadlineText:
                $"THIS DRUG WAS CHOSEN BY THE USER AS PROBLEMATIC AND HAS THIS DESCRIPTION OF A PROBLEM {problem.ProblemNote}"))
            .ToList();

        if (rows.Count == 0)
        {
            return null;
        }

        return new SpecificDrugProblemsSectionReadModel(
            SectionCode: "specific_drug.problems",
            RecordCount: rows.Count,
            Rows: rows);
    }

    private static SpecificDrugAllergiesSectionReadModel? BuildSpecificDrugAllergies(
        DrugFormSelection selection,
        IReadOnlyCollection<ClientDrugAllergy> allergies,
        ReportCompositionContext context)
    {
        SpecificDrugDirectAllergyRowReadModel? direct = allergies
            .FirstOrDefault(allergy => allergy.Source == AllergyRecordSource.Database && allergy.DrugId == selection.Drug.Id) is
            { } directAllergy
            ? new SpecificDrugDirectAllergyRowReadModel(
                ProblemDescription: directAllergy.Note ?? string.Empty,
                HeadlineText:
                $"THIS DRUG WAS MARKED BY THE USER AS ALLERGIC: {directAllergy.Note ?? string.Empty}")
            : null;

        var drugSubstanceIds = selection.Drug.ActiveSubstances.Select(substance => substance.Id).ToHashSet();

        var overlapRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database && allergy.Drug is not null)
            .SelectMany(allergy => allergy.Drug!.ActiveSubstances
                .Where(substance => drugSubstanceIds.Contains(substance.Id))
                .Select(substance => new ActiveSubstanceAllergyOverlapRowReadModel(
                    ActiveSubstanceName: substance.ActiveSubstanceName,
                    SourceAllergyDrugDisplay: allergy.Drug!.CommercialName,
                    UserNote: allergy.Note,
                    HeadlineText:
                    $"THIS ACTIVE SUBSTANCE WAS CHOSEN AS THE ALLERGIC BY THE USER IN ANOTHER DRUG: {allergy.Drug.CommercialName}",
                    DisplayLine:
                    $"{substance.ActiveSubstanceName} <= allergic via {allergy.Drug.CommercialName} | {allergy.Note}")))
            .GroupBy(row => (row.ActiveSubstanceName, row.SourceAllergyDrugDisplay))
            .Select(group => group.First())
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

    private async Task<UserInfoSectionReadModel> BuildUserInfoSectionAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        List<MainWarningReadModel> mainWarnings = await BuildMainWarningsAsync(userId, cancellationToken);
        HealthConditionsGroupReadModel healthConditions = await BuildHealthConditionsGroupAsync(userId, cancellationToken);
        ProblematicDrugsGroupReadModel problematicDrugs = await BuildProblematicDrugsGroupAsync(userId, cancellationToken);
        AllergiesGroupReadModel allergies = await BuildAllergiesGroupAsync(userId, cancellationToken);

        return new UserInfoSectionReadModel(
            MainWarnings: mainWarnings,
            HealthConditions: healthConditions,
            ProblematicDrugs: problematicDrugs,
            Allergies: allergies);
    }

    private async Task<List<MainWarningReadModel>> BuildMainWarningsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        ClientReproductiveHealthProfile? profile =
            await _clientReproductiveHealthProfileRepository.GetTrackedByUserIdAsync(userId, cancellationToken);

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

    private async Task<HealthConditionsGroupReadModel> BuildHealthConditionsGroupAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        List<UserHealthCondition> rows = await LoadAllUserHealthConditionsAsync(userId, cancellationToken);

        var databaseRows = rows
            .Where(row => row.Source == HealthConditionSource.Database && row.HealthCondition is not null)
            .Select(row => new HealthConditionRowReadModel(
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

    private async Task<ProblematicDrugsGroupReadModel> BuildProblematicDrugsGroupAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        List<DrugProblem> problems = await LoadAllDrugProblemsAsync(userId, cancellationToken);

        var databaseRows = problems
            .Where(problem => problem.Source == DrugProblemSource.Database && problem.Drug is not null)
            .Select(problem => new ProblematicDrugRowReadModel(
                DrugDisplayLine: problem.Drug!.CommercialName,
                UserNote: problem.ProblemNote,
                DisplayLine: $"{problem.Drug.CommercialName} | {problem.ProblemNote}"))
            .ToList();

        var customRows = problems
            .Where(problem => problem.Source == DrugProblemSource.Custom)
            .Select(problem => new CustomProblematicDrugRowReadModel(
                CommercialName: problem.CustomDrugName ?? string.Empty,
                UserNote: problem.ProblemNote,
                DisplayLine: $"{problem.CustomDrugName} | {problem.ProblemNote}"))
            .ToList();

        return new ProblematicDrugsGroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "problematic_drugs.from_database",
                DrugInteractionToolHeadlines.ProblematicDrugsDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "problematic_drugs.custom",
                DrugInteractionToolHeadlines.ProblematicDrugsCustom,
                customRows));
    }

    private async Task<AllergiesGroupReadModel> BuildAllergiesGroupAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        List<ClientDrugAllergy> drugAllergies = await LoadAllDrugAllergiesAsync(userId, cancellationToken);
        List<ClientFoodAllergy> foodAllergies = await LoadAllFoodAllergiesAsync(userId, cancellationToken);
        List<ClientSupplementAllergy> supplementAllergies = await LoadAllSupplementAllergiesAsync(userId, cancellationToken);
        List<ClientOtherAllergy> otherAllergies = await LoadAllOtherAllergiesAsync(userId, cancellationToken);
        ClientAllergySummary? summary = await _clientAllergySummaryRepository.GetByUserIdAsync(userId, cancellationToken);
        HashSet<Guid> takenFoodIds = await LoadTakenNowFoodIdsAsync(userId, cancellationToken);
        HashSet<Guid> takenSupplementIds = await LoadTakenNowSupplementIdsAsync(userId, cancellationToken);

        return new AllergiesGroupReadModel(
            DrugAllergies: BuildDrugAllergySubgroup(drugAllergies),
            SupplementAllergies: BuildSupplementAllergySubgroup(supplementAllergies, takenSupplementIds),
            FoodAllergies: BuildFoodAllergySubgroup(foodAllergies, takenFoodIds),
            OtherAllergies: BuildOtherAllergySubgroup(summary, otherAllergies));
    }

    private static AllergyDrugSubgroupReadModel BuildDrugAllergySubgroup(IReadOnlyCollection<ClientDrugAllergy> allergies)
    {
        var databaseRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database && allergy.Drug is not null)
            .Select(allergy => new DrugAllergyRowReadModel(
                DrugDisplayLine: allergy.Drug!.CommercialName,
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.Drug.CommercialName} | {allergy.Note}"))
            .ToList();

        var customRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Custom)
            .Select(allergy => new CustomDrugAllergyRowReadModel(
                CommercialName: allergy.CustomDrugName ?? string.Empty,
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.CustomDrugName} | {allergy.Note}"))
            .ToList();

        return new AllergyDrugSubgroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.drug.from_database",
                DrugInteractionToolHeadlines.DrugAllergiesDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.drug.custom",
                DrugInteractionToolHeadlines.DrugAllergiesCustom,
                customRows));
    }

    private static AllergyFoodSubgroupReadModel BuildFoodAllergySubgroup(
        IReadOnlyCollection<ClientFoodAllergy> allergies,
        HashSet<Guid> takenFoodIds)
    {
        var databaseRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database && allergy.Food is not null)
            .Select(allergy => new CatalogAllergyRowReadModel(
                Name: allergy.Food!.Name,
                AliasNames: allergy.Food.Aliases.ToList(),
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.Food.Name} | {allergy.Note}"))
            .ToList();

        var customRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Custom)
            .Select(allergy => new CustomCatalogAllergyRowReadModel(
                Name: allergy.CustomFoodName ?? string.Empty,
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.CustomFoodName} | {allergy.Note}"))
            .ToList();

        var overlapRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database
                && allergy.FoodId.HasValue
                && takenFoodIds.Contains(allergy.FoodId.Value))
            .Select(allergy => new TakenNowAllergyOverlapRowReadModel(
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
            .Select(allergy => new CatalogAllergyRowReadModel(
                Name: allergy.Supplement!.Name,
                AliasNames: allergy.Supplement.Aliases.ToList(),
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.Supplement.Name} | {allergy.Note}"))
            .ToList();

        var customRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Custom)
            .Select(allergy => new CustomCatalogAllergyRowReadModel(
                Name: allergy.CustomSupplementName ?? string.Empty,
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.CustomSupplementName} | {allergy.Note}"))
            .ToList();

        var overlapRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database
                && allergy.SupplementId.HasValue
                && takenSupplementIds.Contains(allergy.SupplementId.Value))
            .Select(allergy => new TakenNowAllergyOverlapRowReadModel(
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

    private async Task<List<Guid>> LoadAllFoodIdsAsync(CancellationToken cancellationToken)
    {
        List<Guid> ids = new();
        int skip = 0;

        while (true)
        {
            List<Food> page = await _foodRepository.SearchAsync(null, skip, PageSize, cancellationToken);
            if (page.Count == 0)
            {
                break;
            }

            ids.AddRange(page.Select(food => food.Id));
            if (page.Count < PageSize)
            {
                break;
            }

            skip += PageSize;
        }

        return ids;
    }

    private async Task<List<Guid>> LoadAllSupplementIdsAsync(CancellationToken cancellationToken)
    {
        List<Guid> ids = new();
        int skip = 0;

        while (true)
        {
            List<Supplement> page = await _supplementRepository.SearchAsync(null, skip, PageSize, cancellationToken);
            if (page.Count == 0)
            {
                break;
            }

            ids.AddRange(page.Select(supplement => supplement.Id));
            if (page.Count < PageSize)
            {
                break;
            }

            skip += PageSize;
        }

        return ids;
    }

    private async Task<HashSet<Guid>> LoadTakenNowFoodIdsAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<ClientRegularFood> rows = await LoadAllLiveRegularFoodsAsync(userId, cancellationToken);
        return rows.Select(row => row.FoodId).ToHashSet();
    }

    private async Task<HashSet<Guid>> LoadTakenNowSupplementIdsAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<ClientRegularSupplement> rows = await LoadAllLiveRegularSupplementsAsync(userId, cancellationToken);
        return rows
            .Where(row => row.Source == RegularSupplementSource.Database && row.SupplementId.HasValue)
            .Select(row => row.SupplementId!.Value)
            .ToHashSet();
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
