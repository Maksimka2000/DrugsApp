using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
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
            .Select(substance => new DrugToolActiveSubstanceDto(
                substance.Id,
                substance.ActiveSubstanceName))
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
            selectedForm,
            activeSubstanceIds,
            substanceNameById,
            interactionScope,
            userContext,
            cancellationToken);

        DrugToolSupplementInteractionSectionDto supplementSection = await BuildSupplementSectionAsync(
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
            activeSubstanceDtos,
            interactionScope,
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
            rows.Count > 0 ? DrugToolHeadlines.AlcoholSection : string.Empty,
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
            return new DrugToolDrugInteractionSectionDto(string.Empty, null, null);
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

        List<DrugSectionRowBuild> allRows = new();

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

            Guid targetFormId = ResolveTargetDrugFormId(targetDrug, userContext);
            DrugForm targetForm = targetDrug.DrugForms.First(form => form.Id == targetFormId);
            bool includeStomachText = selectedForm.IsOral && targetForm.IsOral;

            var pairs = relevantPairs
                .Select(row => new DrugToolDrugInteractionPairDto(
                    row.LeftPartyId,
                    leftSubstanceNameById[row.LeftPartyId],
                    row.RightPartyId,
                    allSubstanceNames[row.RightPartyId],
                    row.DefaultText ?? string.Empty,
                    includeStomachText ? row.StomachText : null,
                    row.SeverityName))
                .OrderBy(pair => pair.ThisActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(pair => pair.TargetActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(pair => pair.Severity)
                .ToList();

            bool isUserMatch = IsDrugUserMatch(targetDrug.Id, targetSubstanceIds, userContext);

            allRows.Add(new DrugSectionRowBuild(
                targetDrug,
                targetForm,
                isUserMatch,
                pairs));
        }

        return BuildDrugSection(allRows, interactionScope);
    }

    private static DrugToolDrugInteractionSectionDto BuildDrugSection(
        List<DrugSectionRowBuild> allRows,
        DrugToolInteractionScope interactionScope)
    {
        if (allRows.Count == 0)
        {
            return new DrugToolDrugInteractionSectionDto(string.Empty, null, null);
        }

        List<DrugToolDrugInteractionRowDto> MapRows(IEnumerable<DrugSectionRowBuild> rows) =>
            rows
                .OrderBy(row => DrugToolMappings.BuildDrugSummaryText(row.TargetDrug, row.TargetForm), StringComparer.OrdinalIgnoreCase)
                .Select(row => new DrugToolDrugInteractionRowDto(
                    new DrugToolDrugTargetSummaryDto(
                        row.TargetDrug.Id,
                        row.TargetForm.Id,
                        DrugToolMappings.BuildDrugSummaryText(row.TargetDrug, row.TargetForm),
                        row.TargetForm.IsOral),
                    row.Pairs.Count,
                    row.Pairs))
                .ToList();

        List<DrugToolDrugInteractionRowDto> userRowDtos = MapRows(allRows.Where(row => row.IsUserMatch));
        DrugToolDrugSubsectionDto? takenNowSubsection = userRowDtos.Count > 0
            ? new DrugToolDrugSubsectionDto(
                DrugToolMappings.MapSubsectionToApi(DrugToolInteractionSubsectionCode.UserData),
                DrugToolHeadlines.DrugTakenNow,
                userRowDtos.Count,
                userRowDtos)
            : null;

        DrugToolDrugSubsectionDto? allOtherSubsection = null;
        if (interactionScope == DrugToolInteractionScope.All)
        {
            List<DrugToolDrugInteractionRowDto> catalogRowDtos = MapRows(allRows);
            if (catalogRowDtos.Count > 0)
            {
                allOtherSubsection = new DrugToolDrugSubsectionDto(
                    DrugToolMappings.MapSubsectionToApi(DrugToolInteractionSubsectionCode.AllOther),
                    DrugToolHeadlines.DrugAllOther,
                    catalogRowDtos.Count,
                    catalogRowDtos);
            }
        }

        return new DrugToolDrugInteractionSectionDto(
            DrugToolHeadlines.DrugSection,
            DrugToolMappings.WrapSubsection(takenNowSubsection),
            DrugToolMappings.WrapSubsection(allOtherSubsection));
    }

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

    private async Task<DrugToolFoodInteractionSectionDto> BuildFoodSectionAsync(
        DrugForm selectedForm,
        IReadOnlyCollection<Guid> leftSubstanceIds,
        IReadOnlyDictionary<Guid, string> substanceNameById,
        DrugToolInteractionScope interactionScope,
        UserContext userContext,
        CancellationToken cancellationToken)
    {
        List<InteractionExpandedProjection> projections =
            await _interactionRepository.ListExpandedByLeftActiveSubstanceIdsAsync(
                InteractionKind.Food,
                leftSubstanceIds,
                cancellationToken);

        List<FlatFoodInteraction> allInteractions = await BuildFlatFoodInteractionsAsync(
            projections,
            substanceNameById,
            selectedForm.IsOral,
            userContext,
            cancellationToken);

        return BuildFoodSection(allInteractions, interactionScope);
    }

    private async Task<DrugToolSupplementInteractionSectionDto> BuildSupplementSectionAsync(
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

        List<FlatSupplementInteraction> allInteractions = await BuildFlatSupplementInteractionsAsync(
            projections,
            substanceNameById,
            selectedForm.IsOral,
            userContext,
            cancellationToken);

        return BuildSupplementSection(allInteractions, interactionScope);
    }

    private async Task<List<FlatFoodInteraction>> BuildFlatFoodInteractionsAsync(
        List<InteractionExpandedProjection> projections,
        IReadOnlyDictionary<Guid, string> substanceNameById,
        bool selectedFormIsOral,
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

        List<FlatFoodInteraction> results = new();

        foreach (InteractionExpandedProjection projection in projections)
        {
            if (!interactionById.TryGetValue(projection.InteractionId, out Interaction? interaction))
            {
                continue;
            }

            InteractionParty? targetParty = interaction.Parties.FirstOrDefault(party =>
                party.PartyType is InteractionType.Food or InteractionType.FoodGroup);

            if (targetParty is null)
            {
                continue;
            }

            (Guid foodId, string foodName, bool isUserMatch) =
                await ResolveFoodTargetAsync(targetParty, projection.RightPartyId, userContext, cancellationToken);

            results.Add(new FlatFoodInteraction(
                projection.LeftPartyId,
                substanceNameById[projection.LeftPartyId],
                foodId,
                foodName,
                projection.DefaultText ?? string.Empty,
                selectedFormIsOral ? projection.StomachText : null,
                projection.SeverityName,
                isUserMatch));
        }

        return results
            .OrderBy(row => row.FoodName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(row => row.Severity)
            .ToList();
    }

    private async Task<List<FlatSupplementInteraction>> BuildFlatSupplementInteractionsAsync(
        List<InteractionExpandedProjection> projections,
        IReadOnlyDictionary<Guid, string> substanceNameById,
        bool selectedFormIsOral,
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

        List<FlatSupplementInteraction> results = new();

        foreach (InteractionExpandedProjection projection in projections)
        {
            if (!interactionById.TryGetValue(projection.InteractionId, out Interaction? interaction))
            {
                continue;
            }

            InteractionParty? targetParty = interaction.Parties.FirstOrDefault(party =>
                party.PartyType is InteractionType.Supplement or InteractionType.SupplementGroup);

            if (targetParty is null)
            {
                continue;
            }

            (Guid supplementId, string supplementName, bool isUserMatch) =
                await ResolveSupplementTargetAsync(targetParty, projection.RightPartyId, userContext, cancellationToken);

            results.Add(new FlatSupplementInteraction(
                projection.LeftPartyId,
                substanceNameById[projection.LeftPartyId],
                supplementId,
                supplementName,
                projection.DefaultText ?? string.Empty,
                selectedFormIsOral ? projection.StomachText : null,
                projection.SeverityName,
                isUserMatch));
        }

        return results
            .OrderBy(row => row.SupplementName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(row => row.Severity)
            .ToList();
    }

    private static DrugToolFoodInteractionSectionDto BuildFoodSection(
        List<FlatFoodInteraction> allInteractions,
        DrugToolInteractionScope interactionScope)
    {
        if (allInteractions.Count == 0)
        {
            return new DrugToolFoodInteractionSectionDto(string.Empty, null, null);
        }

        (DrugToolFoodSubsectionDto? takenNow, DrugToolFoodSubsectionDto? allOther) =
            BuildFoodSubsections(allInteractions, interactionScope);

        return new DrugToolFoodInteractionSectionDto(
            DrugToolHeadlines.FoodSection,
            DrugToolMappings.WrapSubsection(takenNow),
            DrugToolMappings.WrapSubsection(allOther));
    }

    private static DrugToolSupplementInteractionSectionDto BuildSupplementSection(
        List<FlatSupplementInteraction> allInteractions,
        DrugToolInteractionScope interactionScope)
    {
        if (allInteractions.Count == 0)
        {
            return new DrugToolSupplementInteractionSectionDto(string.Empty, null, null);
        }

        (DrugToolSupplementSubsectionDto? takenNow, DrugToolSupplementSubsectionDto? allOther) =
            BuildSupplementSubsections(allInteractions, interactionScope);

        return new DrugToolSupplementInteractionSectionDto(
            DrugToolHeadlines.SupplementSection,
            DrugToolMappings.WrapSubsection(takenNow),
            DrugToolMappings.WrapSubsection(allOther));
    }

    private static (DrugToolFoodSubsectionDto? TakenNow, DrugToolFoodSubsectionDto? AllOther) BuildFoodSubsections(
        List<FlatFoodInteraction> allInteractions,
        DrugToolInteractionScope interactionScope)
    {
        var userItems = allInteractions
            .Where(row => row.IsUserMatch)
            .ToList();

        DrugToolFoodSubsectionDto? takenNow = userItems.Count > 0
            ? new DrugToolFoodSubsectionDto(
                DrugToolMappings.MapSubsectionToApi(DrugToolInteractionSubsectionCode.UserData),
                userItems.Count,
                userItems.Select(MapFoodInteraction).ToList())
            : null;

        DrugToolFoodSubsectionDto? allOther = null;
        if (interactionScope == DrugToolInteractionScope.All)
        {
            allOther = new DrugToolFoodSubsectionDto(
                DrugToolMappings.MapSubsectionToApi(DrugToolInteractionSubsectionCode.All),
                allInteractions.Count,
                allInteractions.Select(MapFoodInteraction).ToList());
        }

        return (takenNow, allOther);
    }

    private static (DrugToolSupplementSubsectionDto? TakenNow, DrugToolSupplementSubsectionDto? AllOther) BuildSupplementSubsections(
        List<FlatSupplementInteraction> allInteractions,
        DrugToolInteractionScope interactionScope)
    {
        var userItems = allInteractions
            .Where(row => row.IsUserMatch)
            .ToList();

        DrugToolSupplementSubsectionDto? takenNow = userItems.Count > 0
            ? new DrugToolSupplementSubsectionDto(
                DrugToolMappings.MapSubsectionToApi(DrugToolInteractionSubsectionCode.UserData),
                userItems.Count,
                userItems.Select(MapSupplementInteraction).ToList())
            : null;

        DrugToolSupplementSubsectionDto? allOther = null;
        if (interactionScope == DrugToolInteractionScope.All)
        {
            allOther = new DrugToolSupplementSubsectionDto(
                DrugToolMappings.MapSubsectionToApi(DrugToolInteractionSubsectionCode.All),
                allInteractions.Count,
                allInteractions.Select(MapSupplementInteraction).ToList());
        }

        return (takenNow, allOther);
    }

    private static DrugToolFoodInteractionDto MapFoodInteraction(FlatFoodInteraction item) =>
        new(
            item.ActiveSubstanceId,
            item.ActiveSubstanceName,
            item.FoodId,
            item.FoodName,
            item.DefaultText,
            item.StomachText,
            item.Severity);

    private static DrugToolSupplementInteractionDto MapSupplementInteraction(FlatSupplementInteraction item) =>
        new(
            item.ActiveSubstanceId,
            item.ActiveSubstanceName,
            item.SupplementId,
            item.SupplementName,
            item.DefaultText,
            item.StomachText,
            item.Severity);

    private async Task<(Guid FoodId, string FoodName, bool IsUserMatch)> ResolveFoodTargetAsync(
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

            return (targetParty.FoodGroupId.Value, name, isUserMatch);
        }

        Food? food = (await _foodRepository.GetByIdsAsync([expandedRightPartyId], cancellationToken)).FirstOrDefault();
        string foodName = food?.Name ?? "Food";
        bool foodMatch = userContext.FoodIds.Contains(expandedRightPartyId);
        return (expandedRightPartyId, foodName, foodMatch);
    }

    private async Task<(Guid SupplementId, string SupplementName, bool IsUserMatch)> ResolveSupplementTargetAsync(
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

            return (targetParty.SupplementGroupId.Value, name, isUserMatch);
        }

        Supplement? supplement = (await _supplementRepository.GetByIdsAsync([expandedRightPartyId], cancellationToken))
            .FirstOrDefault();
        string supplementName = supplement?.Name ?? "Supplement";
        bool supplementMatch = userContext.SupplementIds.Contains(expandedRightPartyId);
        return (expandedRightPartyId, supplementName, supplementMatch);
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

    private sealed record DrugSectionRowBuild(
        Drug TargetDrug,
        DrugForm TargetForm,
        bool IsUserMatch,
        IReadOnlyCollection<DrugToolDrugInteractionPairDto> Pairs);

    private sealed record FlatFoodInteraction(
        Guid ActiveSubstanceId,
        string ActiveSubstanceName,
        Guid FoodId,
        string FoodName,
        string DefaultText,
        string? StomachText,
        string Severity,
        bool IsUserMatch);

    private sealed record FlatSupplementInteraction(
        Guid ActiveSubstanceId,
        string ActiveSubstanceName,
        Guid SupplementId,
        string SupplementName,
        string DefaultText,
        string? StomachText,
        string Severity,
        bool IsUserMatch);

    private sealed record UserContext(
        List<ClientRegularDrug> RegularDrugs,
        HashSet<Guid> DrugIds,
        HashSet<Guid> FoodIds,
        HashSet<Guid> SupplementIds);
}
