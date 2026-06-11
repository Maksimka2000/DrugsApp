using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using DrugsApp.Domain.MealTimeManagementFeature.Repositories;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Domain.SystemTextFeature.Repositories;
using DrugsApp.Features.DrugToolFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.DrugToolFeature.ApplicationServices;

internal sealed class DrugToolDetailApplicationService(
    IDrugRepository _drugRepository,
    IActiveSubstanceRepository _activeSubstanceRepository,
    IInteractionRepository _interactionRepository,
    ISystemTextRepository _systemTextRepository,
    IClientRegularDrugRepository _clientRegularDrugRepository,
    IClientRegularFoodRepository _clientRegularFoodRepository,
    IClientRegularSupplementRepository _clientRegularSupplementRepository,
    IClientOwnSupplementRepository _clientOwnSupplementRepository,
    IClientMealTimeRepository _clientMealTimeRepository,
    IFoodRepository _foodRepository,
    IFoodGroupRepository _foodGroupRepository,
    ISupplementRepository _supplementRepository,
    ISupplementGroupRepository _supplementGroupRepository) : IDrugToolDetailApplicationService
{
    private const int PageSize = 40;
    private const string AlcoholDefaultTextKey = "activeSubstance.alcohol.defaultText";

    public async Task<ErrorOr<DrugToolDetailBuildResult>> BuildAsync(
        Guid drugId,
        Guid? selectedDrugFormId,
        DrugToolInteractionScope interactionScope,
        Guid userId,
        CancellationToken cancellationToken)
    {
        Drug? drug = await _drugRepository.GetByIdAsync(drugId, cancellationToken);
        if (drug is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "drug",
                    ["resourceId"] = drugId.ToString()
                });
        }

        ErrorOr<DrugForm> selectedFormResult = ResolveSelectedForm(drug, selectedDrugFormId);
        if (selectedFormResult.IsError)
        {
            return selectedFormResult.Errors;
        }

        DrugForm selectedForm = selectedFormResult.Value;
        var activeSubstanceIds = drug.ActiveSubstances.Select(substance => substance.Id).ToList();

        IReadOnlyCollection<ActiveSubstance> loadedSubstances =
            await _activeSubstanceRepository.GetByIdsAsync(activeSubstanceIds, cancellationToken);
        var activeSubstanceById =
            loadedSubstances.ToDictionary(substance => substance.Id);

        UserContext userContext = await BuildUserContextAsync(userId, cancellationToken);

        IReadOnlyCollection<DrugToolActiveSubstanceDto> activeSubstanceDtos = drug.ActiveSubstances
            .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(substance => substance.Id)
            .Select(substance =>
            {
                activeSubstanceById.TryGetValue(substance.Id, out ActiveSubstance? loaded);
                return new DrugToolActiveSubstanceDto(
                    substance.Id,
                    substance.ActiveSubstanceName,
                    loaded is null ? null : DrugToolMappings.FormatOralGuidanceText(loaded.OralMedicationIntakeGuidelines));
            })
            .ToList();

        IReadOnlyCollection<DrugToolAvailableDrugFormDto> availableForms = drug.DrugForms
            .OrderBy(form => DrugToolMappings.BuildFormLabel(form), StringComparer.OrdinalIgnoreCase)
            .ThenBy(form => form.Id)
            .Select(form => new DrugToolAvailableDrugFormDto(
                form.Id,
                DrugToolMappings.BuildFormLabel(form),
                form.IsOral,
                form.Dosage ?? string.Empty,
                form.PharmaceuticalForms
                    .OrderBy(pf => pf.FormulationName, StringComparer.OrdinalIgnoreCase)
                    .Select(pf => pf.FormulationName)
                    .FirstOrDefault() ?? string.Empty))
            .ToList();

        List<ClientMealTime> mealTimes = await _clientMealTimeRepository.ListByUserAsync(userId, cancellationToken);
        IReadOnlyCollection<DrugToolUserMealTimeDto> userMealTimeDtos = mealTimes
            .OrderBy(meal => meal.IntakeTime)
            .ThenBy(meal => meal.MealName, StringComparer.OrdinalIgnoreCase)
            .Select(meal => new DrugToolUserMealTimeDto(meal.MealName, meal.IntakeTime.ToString("HH:mm")))
            .ToList();

        DrugToolAlcoholSectionDto alcoholSection = await BuildAlcoholSectionAsync(
            drug,
            activeSubstanceById,
            cancellationToken);

        var substanceNameById = drug.ActiveSubstances.ToDictionary(
            substance => substance.Id,
            substance => substance.ActiveSubstanceName);

        DrugToolDrugInteractionSectionDto drugSection = await BuildDrugSectionAsync(
            drug,
            selectedForm,
            activeSubstanceIds,
            substanceNameById,
            interactionScope,
            userContext,
            cancellationToken);

        DrugToolFoodInteractionSectionDto foodSection = await BuildFoodSectionAsync(
            drug,
            selectedForm,
            activeSubstanceIds,
            substanceNameById,
            interactionScope,
            userContext,
            userMealTimeDtos,
            activeSubstanceById,
            cancellationToken);

        DrugToolSupplementInteractionSectionDto supplementSection = await BuildSupplementSectionAsync(
            drug,
            selectedForm,
            activeSubstanceIds,
            substanceNameById,
            interactionScope,
            userContext,
            cancellationToken);

        return new DrugToolDetailBuildResult(
            drug.Id,
            drug.CommercialName,
            drug.TypeOfProduct,
            drug.TypeOfDrug,
            selectedForm.Id,
            DrugToolMappings.MapFormSummary(selectedForm),
            DrugToolMappings.BuildDrugDisplayLine(drug, selectedForm),
            activeSubstanceDtos,
            availableForms,
            interactionScope,
            userMealTimeDtos,
            alcoholSection,
            drugSection,
            foodSection,
            supplementSection);
    }

    private static ErrorOr<DrugForm> ResolveSelectedForm(Drug drug, Guid? selectedDrugFormId)
    {
        if (selectedDrugFormId is null)
        {
            DrugForm? firstForm = drug.DrugForms
                .OrderBy(form => DrugToolMappings.BuildFormLabel(form), StringComparer.OrdinalIgnoreCase)
                .ThenBy(form => form.Id)
                .FirstOrDefault();

            if (firstForm is null)
            {
                return Error.Validation(
                    description: "Drug has no available forms.",
                    metadata: new Dictionary<string, object> { ["drugId"] = drug.Id.ToString() });
            }

            return firstForm;
        }

        DrugForm? selectedForm = drug.DrugForms.FirstOrDefault(form => form.Id == selectedDrugFormId.Value);
        if (selectedForm is null)
        {
            return Error.Validation(
                description: "Selected drug form does not belong to the requested drug.",
                metadata: new Dictionary<string, object>
                {
                    ["drugId"] = drug.Id.ToString(),
                    ["selectedDrugFormId"] = selectedDrugFormId.Value.ToString()
                });
        }

        return selectedForm;
    }

    private async Task<DrugToolAlcoholSectionDto> BuildAlcoholSectionAsync(
        Drug drug,
        IReadOnlyDictionary<Guid, ActiveSubstance> activeSubstanceById,
        CancellationToken cancellationToken)
    {
        string defaultAlcoholText =
            (await _systemTextRepository.GetByKeyAsync(AlcoholDefaultTextKey, cancellationToken))?.Value
            ?? string.Empty;

        List<DrugToolAlcoholRowDto> rows = new();

        foreach (ActiveSubstance substance in drug.ActiveSubstances
                     .OrderBy(s => s.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(s => s.Id))
        {
            if (!activeSubstanceById.TryGetValue(substance.Id, out ActiveSubstance? loaded) || !loaded.HasAlcoholInteraction)
            {
                continue;
            }

            string? specialText = loaded.AlcoholInteractionOverride?.AlcoholSpecialText;
            string effectiveSummary = string.IsNullOrWhiteSpace(specialText)
                ? defaultAlcoholText
                : $"{defaultAlcoholText} {specialText}".Trim();

            rows.Add(new DrugToolAlcoholRowDto(
                substance.Id,
                substance.ActiveSubstanceName,
                defaultAlcoholText,
                specialText,
                effectiveSummary));
        }

        return new DrugToolAlcoholSectionDto(
            rows.Count,
            rows.Count > 0 ? DrugToolMappings.AlcoholHeadline : string.Empty,
            rows);
    }

    private async Task<DrugToolDrugInteractionSectionDto> BuildDrugSectionAsync(
        Drug drug,
        DrugForm selectedForm,
        IReadOnlyCollection<Guid> leftSubstanceIds,
        IReadOnlyDictionary<Guid, string> leftSubstanceNameById,
        DrugToolInteractionScope interactionScope,
        UserContext userContext,
        CancellationToken cancellationToken)
    {
        List<InteractionExpandedProjection> projections =
            await _interactionRepository.ListExpandedByLeftActiveSubstanceIdsAsync(
                InteractionKind.Substance,
                leftSubstanceIds,
                cancellationToken);

        if (projections.Count == 0)
        {
            return new DrugToolDrugInteractionSectionDto(0, []);
        }

        var rightSubstanceIds = projections
            .Where(row => row.RightPartyType == ShortInteractionType.Substance)
            .Select(row => row.RightPartyId)
            .ToHashSet();

        List<Drug> targetDrugs = await _drugRepository.ListByActiveSubstanceIdsAsync(
            rightSubstanceIds,
            excludeDrugId: null,
            cancellationToken);

        var targetDrugById = targetDrugs.ToDictionary(target => target.Id);
        if (rightSubstanceIds.Overlaps(drug.ActiveSubstances.Select(s => s.Id)))
        {
            targetDrugById[drug.Id] = drug;
        }

        var allSubstanceNames = targetDrugs
            .SelectMany(target => target.ActiveSubstances)
            .GroupBy(substance => substance.Id)
            .ToDictionary(group => group.Key, group => group.First().ActiveSubstanceName);

        foreach ((Guid id, string name) in leftSubstanceNameById)
        {
            allSubstanceNames[id] = name;
        }

        List<DrugToolDrugInteractionGroupDto> allGroups = new();

        foreach (Drug targetDrug in targetDrugById.Values)
        {
            var targetSubstanceIds = targetDrug.ActiveSubstances.Select(s => s.Id).ToHashSet();
            var relevantPairs = projections
                .Where(row => row.RightPartyType == ShortInteractionType.Substance && targetSubstanceIds.Contains(row.RightPartyId))
                .ToList();

            if (relevantPairs.Count == 0)
            {
                continue;
            }

            bool isCurrentDrug = targetDrug.Id == drug.Id;
            Guid targetFormId = ResolveTargetDrugFormId(targetDrug, userContext);
            DrugForm targetForm = targetDrug.DrugForms.First(form => form.Id == targetFormId);
            bool includeStomachText = selectedForm.IsOral && targetForm.IsOral;

            var pairs = relevantPairs
                .Select(row => new DrugToolDrugInteractionPairDto(
                    leftSubstanceNameById[row.LeftPartyId],
                    allSubstanceNames[row.RightPartyId],
                    row.DefaultText ?? string.Empty,
                    includeStomachText ? row.StomachText : null,
                    row.SeverityName,
                    targetFormId,
                    targetForm.IsOral))
                .OrderBy(pair => pair.LeftActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(pair => pair.RightActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(pair => pair.Severity)
                .ToList();

            bool isUserMatch = IsDrugUserMatch(targetDrug.Id, targetSubstanceIds, userContext);

            allGroups.Add(new DrugToolDrugInteractionGroupDto(
                targetDrug.Id,
                isCurrentDrug ? DrugToolMappings.ThisDrugLabel : targetDrug.CommercialName,
                isCurrentDrug
                    ? DrugToolMappings.ThisDrugLabel
                    : DrugToolMappings.BuildDrugDisplayLine(targetDrug, targetForm),
                isCurrentDrug,
                isUserMatch,
                MapTargetForms(targetDrug),
                pairs));
        }

        return BuildDrugSubsections(allGroups, interactionScope);
    }

    private static DrugToolDrugInteractionSectionDto BuildDrugSubsections(
        List<DrugToolDrugInteractionGroupDto> allGroups,
        DrugToolInteractionScope interactionScope)
    {
        var orderedGroups = allGroups
            .OrderByDescending(group => group.IsUserMatch)
            .ThenBy(group => group.TargetDisplayLine, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (interactionScope == DrugToolInteractionScope.UserOnly)
        {
            orderedGroups = orderedGroups
                .Where(group => group.IsUserMatch)
                .ToList();
        }

        List<DrugToolInteractionSubsectionDto> subsections = new();

        var userMatches = orderedGroups
            .Where(group => group.IsUserMatch)
            .ToList();

        if (userMatches.Count > 0)
        {
            subsections.Add(new DrugToolInteractionSubsectionDto(
                DrugToolMappings.MapSubsectionToApi(DrugToolInteractionSubsectionCode.UserMatches),
                userMatches.Count,
                userMatches));
        }

        if (interactionScope == DrugToolInteractionScope.All)
        {
            var remainder = orderedGroups
                .Where(group => !group.IsUserMatch)
                .ToList();

            if (remainder.Count > 0)
            {
                subsections.Add(new DrugToolInteractionSubsectionDto(
                    DrugToolMappings.MapSubsectionToApi(DrugToolInteractionSubsectionCode.CatalogRemainder),
                    remainder.Count,
                    remainder));
            }
        }

        return new DrugToolDrugInteractionSectionDto(orderedGroups.Count, subsections);
    }

    private async Task<DrugToolFoodInteractionSectionDto> BuildFoodSectionAsync(
        Drug drug,
        DrugForm selectedForm,
        IReadOnlyCollection<Guid> leftSubstanceIds,
        IReadOnlyDictionary<Guid, string> substanceNameById,
        DrugToolInteractionScope interactionScope,
        UserContext userContext,
        IReadOnlyCollection<DrugToolUserMealTimeDto> userMealTimes,
        IReadOnlyDictionary<Guid, ActiveSubstance> activeSubstanceById,
        CancellationToken cancellationToken)
    {
        List<InteractionExpandedProjection> projections =
            await _interactionRepository.ListExpandedByLeftActiveSubstanceIdsAsync(
                InteractionKind.Food,
                leftSubstanceIds,
                cancellationToken);

        List<DrugToolFoodSupplementInteractionGroupDto> allGroups = await BuildFoodOrSupplementGroupsAsync(
            projections,
            substanceNameById,
            selectedForm.IsOral,
            isFood: true,
            userContext,
            cancellationToken);

        DrugToolOralGuidanceDto? oralGuidance = selectedForm.IsOral
            ? BuildOralGuidance(drug, activeSubstanceById, userMealTimes)
            : null;

        int foodRecordCount = interactionScope == DrugToolInteractionScope.UserOnly
            ? allGroups.Count(group => group.IsUserMatch)
            : allGroups.Count;

        return new DrugToolFoodInteractionSectionDto(
            foodRecordCount,
            oralGuidance,
            BuildFoodSupplementSubsections(allGroups, interactionScope));
    }

    private async Task<DrugToolSupplementInteractionSectionDto> BuildSupplementSectionAsync(
        Drug drug,
        DrugForm selectedForm,
        IReadOnlyCollection<Guid> leftSubstanceIds,
        IReadOnlyDictionary<Guid, string> substanceNameById,
        DrugToolInteractionScope interactionScope,
        UserContext userContext,
        CancellationToken cancellationToken)
    {
        List<InteractionExpandedProjection> projections =
            await _interactionRepository.ListExpandedByLeftActiveSubstanceIdsAsync(
                InteractionKind.Supplement,
                leftSubstanceIds,
                cancellationToken);

        List<DrugToolFoodSupplementInteractionGroupDto> allGroups = await BuildFoodOrSupplementGroupsAsync(
            projections,
            substanceNameById,
            selectedForm.IsOral,
            isFood: false,
            userContext,
            cancellationToken);

        int supplementRecordCount = interactionScope == DrugToolInteractionScope.UserOnly
            ? allGroups.Count(group => group.IsUserMatch)
            : allGroups.Count;

        return new DrugToolSupplementInteractionSectionDto(
            supplementRecordCount,
            BuildFoodSupplementSubsections(allGroups, interactionScope));
    }

    private async Task<List<DrugToolFoodSupplementInteractionGroupDto>> BuildFoodOrSupplementGroupsAsync(
        List<InteractionExpandedProjection> projections,
        IReadOnlyDictionary<Guid, string> substanceNameById,
        bool selectedFormIsOral,
        bool isFood,
        UserContext userContext,
        CancellationToken cancellationToken)
    {
        if (projections.Count == 0)
        {
            return [];
        }

        var interactionIds = projections.Select(row => row.InteractionId).ToHashSet();
        List<Interaction> interactions = await _interactionRepository.GetByIdsAsync(interactionIds, cancellationToken);
        var interactionById = interactions.ToDictionary(interaction => interaction.Id);

        Dictionary<string, DrugToolFoodSupplementInteractionGroupDto> groups = new(StringComparer.Ordinal);

        foreach (InteractionExpandedProjection projection in projections)
        {
            if (!interactionById.TryGetValue(projection.InteractionId, out Interaction? interaction))
            {
                continue;
            }

            InteractionParty? targetParty = interaction.Parties.FirstOrDefault(party =>
                isFood
                    ? party.PartyType is InteractionType.Food or InteractionType.FoodGroup
                    : party.PartyType is InteractionType.Supplement or InteractionType.SupplementGroup);

            if (targetParty is null)
            {
                continue;
            }

            string targetEntityType;
            string targetDisplayName;
            Guid targetKeyId;
            bool isUserMatch;

            if (isFood)
            {
                (targetEntityType, targetDisplayName, targetKeyId, isUserMatch) =
                    await ResolveFoodTargetAsync(targetParty, projection.RightPartyId, userContext, cancellationToken);
            }
            else
            {
                (targetEntityType, targetDisplayName, targetKeyId, isUserMatch) =
                    await ResolveSupplementTargetAsync(targetParty, projection.RightPartyId, userContext, cancellationToken);
            }

            string groupKey = $"{targetEntityType}:{targetKeyId}";
            if (!groups.TryGetValue(groupKey, out DrugToolFoodSupplementInteractionGroupDto? existing))
            {
                existing = new DrugToolFoodSupplementInteractionGroupDto(
                    targetEntityType,
                    targetDisplayName,
                    isUserMatch,
                    []);
                groups[groupKey] = existing;
            }

            DrugToolFoodSupplementInteractionPairDto pair = new(
                substanceNameById[projection.LeftPartyId],
                targetDisplayName,
                projection.DefaultText ?? string.Empty,
                selectedFormIsOral ? projection.StomachText : null,
                projection.SeverityName);

            var updatedPairs = existing.InteractionPairs
                .Append(pair)
                .OrderBy(p => p.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.TargetDisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(p => p.Severity)
                .ToList();

            groups[groupKey] = existing with
            {
                IsUserMatch = existing.IsUserMatch || isUserMatch,
                InteractionPairs = updatedPairs
            };
        }

        return groups.Values
            .OrderBy(group => group.TargetDisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<(string EntityType, string DisplayName, Guid KeyId, bool IsUserMatch)> ResolveFoodTargetAsync(
        InteractionParty targetParty,
        Guid expandedRightPartyId,
        UserContext userContext,
        CancellationToken cancellationToken)
    {
        if (targetParty.PartyType == InteractionType.FoodGroup && targetParty.FoodGroupId.HasValue)
        {
            FoodGroup? group = (await _foodGroupRepository.GetByIdsAsync(
                [targetParty.FoodGroupId.Value],
                cancellationToken)).FirstOrDefault();

            string name = group?.Name ?? "Food group";
            bool isUserMatch = group?.Foods.Any(food => userContext.FoodIds.Contains(food.Id)) == true
                || userContext.FoodIds.Contains(expandedRightPartyId);

            return ("food_group", name, targetParty.FoodGroupId.Value, isUserMatch);
        }

        Food? food = (await _foodRepository.GetByIdsAsync([expandedRightPartyId], cancellationToken)).FirstOrDefault();
        string foodName = food?.Name ?? "Food";
        bool foodMatch = userContext.FoodIds.Contains(expandedRightPartyId);
        return ("food", foodName, expandedRightPartyId, foodMatch);
    }

    private async Task<(string EntityType, string DisplayName, Guid KeyId, bool IsUserMatch)> ResolveSupplementTargetAsync(
        InteractionParty targetParty,
        Guid expandedRightPartyId,
        UserContext userContext,
        CancellationToken cancellationToken)
    {
        if (targetParty.PartyType == InteractionType.SupplementGroup && targetParty.SupplementGroupId.HasValue)
        {
            SupplementGroup? group = (await _supplementGroupRepository.GetByIdsAsync(
                [targetParty.SupplementGroupId.Value],
                cancellationToken)).FirstOrDefault();

            string name = group?.Name ?? "Supplement group";
            bool isUserMatch = group?.Supplements.Any(supplement => userContext.SupplementIds.Contains(supplement.Id)) == true
                || userContext.SupplementIds.Contains(expandedRightPartyId);

            return ("supplement_group", name, targetParty.SupplementGroupId.Value, isUserMatch);
        }

        Supplement? supplement = (await _supplementRepository.GetByIdsAsync([expandedRightPartyId], cancellationToken))
            .FirstOrDefault();
        string supplementName = supplement?.Name ?? "Supplement";
        bool supplementMatch = userContext.SupplementIds.Contains(expandedRightPartyId);
        return ("supplement", supplementName, expandedRightPartyId, supplementMatch);
    }

    private static List<DrugToolFoodSupplementSubsectionDto> BuildFoodSupplementSubsections(
        List<DrugToolFoodSupplementInteractionGroupDto> allGroups,
        DrugToolInteractionScope interactionScope)
    {
        if (interactionScope == DrugToolInteractionScope.UserOnly)
        {
            allGroups = allGroups
                .Where(group => group.IsUserMatch)
                .ToList();
        }

        List<DrugToolFoodSupplementSubsectionDto> subsections = new();

        var userMatches = allGroups
            .Where(group => group.IsUserMatch)
            .ToList();

        if (userMatches.Count > 0)
        {
            subsections.Add(new DrugToolFoodSupplementSubsectionDto(
                DrugToolMappings.MapSubsectionToApi(DrugToolInteractionSubsectionCode.UserMatches),
                userMatches.Count,
                userMatches));
        }

        if (interactionScope == DrugToolInteractionScope.All)
        {
            var remainder = allGroups
                .Where(group => !group.IsUserMatch)
                .ToList();

            if (remainder.Count > 0)
            {
                subsections.Add(new DrugToolFoodSupplementSubsectionDto(
                    DrugToolMappings.MapSubsectionToApi(DrugToolInteractionSubsectionCode.CatalogRemainder),
                    remainder.Count,
                    remainder));
            }
        }

        return subsections;
    }

    private static DrugToolOralGuidanceDto BuildOralGuidance(
        Drug drug,
        IReadOnlyDictionary<Guid, ActiveSubstance> activeSubstanceById,
        IReadOnlyCollection<DrugToolUserMealTimeDto> userMealTimes)
    {
        string mealSummary = userMealTimes.Count > 0
            ? $"Your meal intake is now: {string.Join(", ", userMealTimes.Select(meal => meal.MealName))}"
            : "You have not configured any meal times yet.";

        var rows = drug.ActiveSubstances
            .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .Select(substance =>
            {
                activeSubstanceById.TryGetValue(substance.Id, out ActiveSubstance? loaded);
                return new DrugToolOralGuidanceSubstanceRowDto(
                    substance.ActiveSubstanceName,
                    DrugToolMappings.FormatOralGuidanceText(loaded?.OralMedicationIntakeGuidelines) ?? string.Empty);
            })
            .ToList();

        return new DrugToolOralGuidanceDto(
            DrugToolMappings.OralGuidanceHeadline,
            mealSummary,
            rows);
    }

    private static IReadOnlyCollection<DrugToolTargetFormDto> MapTargetForms(Drug targetDrug) =>
        targetDrug.DrugForms
            .OrderBy(form => DrugToolMappings.BuildFormLabel(form), StringComparer.OrdinalIgnoreCase)
            .ThenBy(form => form.Id)
            .Select(form => new DrugToolTargetFormDto(
                form.PharmaceuticalForms
                    .OrderBy(pf => pf.FormulationName, StringComparer.OrdinalIgnoreCase)
                    .Select(pf => pf.FormulationName)
                    .FirstOrDefault() ?? string.Empty,
                form.Dosage ?? string.Empty,
                form.IsOral))
            .ToList();

    private static Guid ResolveTargetDrugFormId(Drug targetDrug, UserContext userContext)
    {
        ClientRegularDrug? matchedRegular = userContext.RegularDrugs
            .FirstOrDefault(row => row.DrugId == targetDrug.Id);

        if (matchedRegular is not null && targetDrug.DrugForms.Any(form => form.Id == matchedRegular.DrugFormId))
        {
            return matchedRegular.DrugFormId;
        }

        return targetDrug.DrugForms
            .OrderBy(form => DrugToolMappings.BuildFormLabel(form), StringComparer.OrdinalIgnoreCase)
            .ThenBy(form => form.Id)
            .First()
            .Id;
    }

    private static bool IsDrugUserMatch(Guid targetDrugId, HashSet<Guid> targetSubstanceIds, UserContext userContext)
    {
        if (userContext.DrugIds.Contains(targetDrugId))
        {
            return true;
        }

        return userContext.RegularDrugs.Any(row =>
            row.Drug.ActiveSubstances.Any(substance => targetSubstanceIds.Contains(substance.Id)));
    }

    private async Task<UserContext> BuildUserContextAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<ClientRegularDrug> regularDrugs = await LoadAllPagesAsync(
            (skip, take, token) => _clientRegularDrugRepository.SearchLiveByUserIdAsync(
                userId, null, null, skip, take, token),
            cancellationToken);

        List<ClientRegularFood> regularFoods = await LoadAllPagesAsync(
            (skip, take, token) => _clientRegularFoodRepository.SearchLiveByUserIdAsync(
                userId, null, null, skip, take, token),
            cancellationToken);

        List<ClientRegularSupplement> regularSupplements = await LoadAllPagesAsync(
            (skip, take, token) => _clientRegularSupplementRepository.SearchLiveByUserIdAsync(
                userId, null, null, skip, take, token),
            cancellationToken);

        HashSet<Guid> supplementIds = new();
        foreach (ClientRegularSupplement row in regularSupplements)
        {
            if (row.Source == RegularSupplementSource.Database && row.SupplementId.HasValue)
            {
                supplementIds.Add(row.SupplementId.Value);
                continue;
            }

            if (row.Source == RegularSupplementSource.Own && row.OwnSupplementId.HasValue)
            {
                ClientOwnSupplement? own = await _clientOwnSupplementRepository.GetByIdForUserWithMembersAsync(
                    userId,
                    row.OwnSupplementId.Value,
                    cancellationToken);

                if (own is not null)
                {
                    foreach (ClientOwnSupplementItem item in own.Items)
                    {
                        supplementIds.Add(item.SupplementId);
                    }
                }
            }
        }

        return new UserContext(
            regularDrugs,
            regularDrugs.Select(row => row.DrugId).ToHashSet(),
            regularFoods.Select(row => row.FoodId).ToHashSet(),
            supplementIds);
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

    private sealed record UserContext(
        List<ClientRegularDrug> RegularDrugs,
        HashSet<Guid> DrugIds,
        HashSet<Guid> FoodIds,
        HashSet<Guid> SupplementIds);
}
