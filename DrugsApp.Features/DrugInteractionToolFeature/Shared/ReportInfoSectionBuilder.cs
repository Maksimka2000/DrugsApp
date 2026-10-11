using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Drug form that entered the report, with the role it plays and its live regular schedule when taken now [description].
/// </summary>
/// <param name="Drug">Loaded drug with active substances.</param>
/// <param name="Form">Selected or taken drug form.</param>
/// <param name="Source">How the form entered the report; null on public reports, where every form is selected.</param>
/// <param name="RegularIntake">Schedule from the live regular-drug row, or null when the form is only selected.</param>
internal sealed record InfoDrugSelection(
    Drug Drug,
    DrugForm Form,
    SelectedDrugSource? Source,
    RegularDrugIntakeReadModel? RegularIntake);

/// <summary>
/// Pure builder of the report info section that lists everything the report was computed from [description].
/// </summary>
internal static class ReportInfoSectionBuilder
{
    /// <summary>
    /// Builds the info section from the effective drug selections and the user's Active regular supplements and foods.
    /// </summary>
    /// <param name="selections">Effective drug selections (manual plus merged regular drugs).</param>
    /// <param name="includeUserData">
    /// Whether user data is part of the report; without it only selected drugs are listed and every taken-now part is null.
    /// </param>
    /// <param name="regularSupplements">Active regular supplements with catalog supplement and own supplement members loaded.</param>
    /// <param name="regularFoods">Active regular foods with catalog food loaded.</param>
    /// <param name="selectedSupplements">Catalog supplements selected for the report.</param>
    /// <param name="selectedFoods">Catalog foods selected for the report.</param>
    /// <param name="audience">Report audience; providers see "selected for prescription" headlines.</param>
    /// <returns>Info section with drugs, supplements, own supplements, and foods.</returns>
    internal static ReportInfoSectionReadModel Build(
        IReadOnlyCollection<InfoDrugSelection> selections,
        bool includeUserData,
        IReadOnlyCollection<ClientRegularSupplement> regularSupplements,
        IReadOnlyCollection<ClientRegularFood> regularFoods,
        IReadOnlyCollection<Supplement>? selectedSupplements = null,
        IReadOnlyCollection<Food>? selectedFoods = null,
        ReportAudience audience = ReportAudience.Registered)
    {
        return new ReportInfoSectionReadModel(
            SectionCode: "info",
            Drugs: BuildDrugsSection(selections, includeUserData, audience),
            Supplements: BuildSupplementsSection(
                regularSupplements,
                selectedSupplements ?? [],
                includeUserData,
                audience),
            Foods: BuildFoodsSection(regularFoods, selectedFoods ?? [], includeUserData, audience));
    }

    private static InfoDrugsSectionReadModel BuildDrugsSection(
        IReadOnlyCollection<InfoDrugSelection> selections,
        bool includeUserData,
        ReportAudience audience)
    {
        List<InfoDrugRowReadModel> selectedRows = selections
            .Where(selection => selection.Source is null
                or SelectedDrugSource.Manual
                or SelectedDrugSource.ManualAndUserRegular)
            .Select(selection => BuildDrugRow(selection, regularIntake: null))
            .OrderBy(row => row.DrugSummaryText, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.DrugFormId)
            .ToList();

        ReportSubsectionReadModel<InfoDrugRowReadModel>? takenNow = null;
        ReportSubsectionReadModel<InfoDrugRowReadModel>? selectedTwice = null;

        if (includeUserData)
        {
            List<InfoDrugRowReadModel> takenNowRows = selections
                .Where(selection => selection.Source is SelectedDrugSource.UserRegular
                    or SelectedDrugSource.ManualAndUserRegular)
                .Select(selection => BuildDrugRow(selection, selection.RegularIntake))
                .OrderBy(row => row.DrugSummaryText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.DrugFormId)
                .ToList();

            takenNow = DrugInteractionToolReportMapping.BuildSubsection(
                "info.drugs.taken_now",
                DrugInteractionToolHeadlines.InfoDrugsTakenNow,
                takenNowRows);

            List<InfoDrugRowReadModel> selectedTwiceRows = selections
                .Where(selection => selection.Source is SelectedDrugSource.ManualAndUserRegular)
                .Select(selection => BuildDrugRow(selection, selection.RegularIntake))
                .OrderBy(row => row.DrugSummaryText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.DrugFormId)
                .ToList();

            selectedTwice = DrugInteractionToolReportMapping.BuildSubsection(
                "info.drugs.selected_twice",
                DrugInteractionToolHeadlines.InfoDrugsSelectedTwice,
                selectedTwiceRows);
        }

        return new InfoDrugsSectionReadModel(
            SectionCode: "info.drugs",
            TakenNow: takenNow,
            Selected: DrugInteractionToolReportMapping.BuildSubsection(
                "info.drugs.selected",
                audience == ReportAudience.Provider
                    ? DrugInteractionToolHeadlines.InfoDrugsSelectedPrescription
                    : DrugInteractionToolHeadlines.InfoDrugsSelected,
                selectedRows),
            SelectedTwice: selectedTwice);
    }

    private static InfoDrugRowReadModel BuildDrugRow(
        InfoDrugSelection selection,
        RegularDrugIntakeReadModel? regularIntake)
    {
        SpecificDrugDetailReadModel detail = DrugInteractionToolReportMapping.MapSpecificDrugDetail(
            selection.Drug,
            selection.Form);

        return new InfoDrugRowReadModel(
            DrugId: selection.Drug.Id,
            DrugFormId: selection.Form.Id,
            CommercialName: selection.Drug.CommercialName,
            DrugSummaryText: DrugInteractionToolReportMapping.BuildDrugSummaryText(selection.Drug, selection.Form),
            FormulationName: detail.SelectedDrugFormSummary.FormulationName,
            Dosage: detail.SelectedDrugFormSummary.Dosage,
            IsOral: selection.Form.IsOral,
            ActiveSubstances: detail.ActiveSubstances,
            RegularIntake: regularIntake);
    }

    private static ReportSubsectionReadModel<InfoSelectedItemRowReadModel> BuildSelectedSubsection(
        string sectionCode,
        string headline,
        IEnumerable<(Guid Id, string Name, IReadOnlyCollection<string> AliasNames, bool? IsAlsoTakenNow)> items)
    {
        List<InfoSelectedItemRowReadModel> rows = items
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Id)
            .Select(item => new InfoSelectedItemRowReadModel(
                item.Id,
                item.Name,
                item.AliasNames,
                item.IsAlsoTakenNow,
                DrugInteractionToolReportMapping.BuildNameWithAliases(item.Name, item.AliasNames)))
            .ToList();

        return DrugInteractionToolReportMapping.BuildSubsection(sectionCode, headline, rows);
    }

    private static InfoSupplementsSectionReadModel BuildSupplementsSection(
        IReadOnlyCollection<ClientRegularSupplement> regularSupplements,
        IReadOnlyCollection<Supplement> selectedSupplements,
        bool includeUserData,
        ReportAudience audience)
    {
        ReportSubsectionReadModel<InfoSelectedItemRowReadModel> selected;
        ReportSubsectionReadModel<InfoSupplementSelectedTwiceRowReadModel>? selectedTwice = null;

        string selectedHeadline = audience == ReportAudience.Provider
            ? DrugInteractionToolHeadlines.InfoSupplementsSelectedPrescription
            : DrugInteractionToolHeadlines.InfoSupplementsSelected;

        if (!includeUserData)
        {
            selected = BuildSelectedSubsection(
                "info.supplements.selected",
                selectedHeadline,
                selectedSupplements.Select(item => (
                    item.Id,
                    item.Name ?? string.Empty,
                    (IReadOnlyCollection<string>)item.Aliases.ToList(),
                    (bool?)null)));

            return new InfoSupplementsSectionReadModel(
                SectionCode: "info.supplements",
                TakenNow: null,
                OwnSupplements: null,
                Selected: selected,
                SelectedTwice: null);
        }

        HashSet<Guid> directSupplementIds = regularSupplements
            .Where(row => row.Source == RegularSupplementSource.Database && row.Supplement is not null)
            .Select(row => row.Supplement!.Id)
            .ToHashSet();
        IReadOnlyDictionary<Guid, IReadOnlyList<OwnSupplementSource>> ownSources =
            OwnSupplementMembershipBuilder.BuildSourcesBySupplementId(regularSupplements);

        selected = BuildSelectedSubsection(
            "info.supplements.selected",
            selectedHeadline,
            selectedSupplements.Select(item => (
                item.Id,
                item.Name ?? string.Empty,
                (IReadOnlyCollection<string>)item.Aliases.ToList(),
                (bool?)(directSupplementIds.Contains(item.Id) || ownSources.ContainsKey(item.Id)))));

        Dictionary<Guid, RegularDrugIntakeReadModel> directIntakeBySupplementId = regularSupplements
            .Where(row => row.Source == RegularSupplementSource.Database && row.Supplement is not null)
            .GroupBy(row => row.Supplement!.Id)
            .ToDictionary(
                group => group.Key,
                group => DrugInteractionToolReportMapping.MapRegularIntakeSnapshot(group.First()));

        List<InfoSupplementSelectedTwiceRowReadModel> selectedTwiceRows = selectedSupplements
            .Where(item => directSupplementIds.Contains(item.Id) || ownSources.ContainsKey(item.Id))
            .Select(item =>
            {
                IReadOnlyCollection<string> aliasNames = item.Aliases.ToList();
                IReadOnlyCollection<InfoOwnSupplementRefReadModel> ownSupplements =
                    ownSources.TryGetValue(item.Id, out IReadOnlyList<OwnSupplementSource>? sources)
                        ? sources
                            .Select(source => new InfoOwnSupplementRefReadModel(
                                source.OwnSupplementId,
                                source.OwnSupplementName))
                            .ToList()
                        : [];

                return new InfoSupplementSelectedTwiceRowReadModel(
                    SupplementId: item.Id,
                    SupplementName: item.Name ?? string.Empty,
                    AliasNames: aliasNames,
                    IsTakenDirectly: directSupplementIds.Contains(item.Id),
                    OwnSupplements: ownSupplements,
                    RegularIntake: directIntakeBySupplementId.GetValueOrDefault(item.Id),
                    DisplayLine: DrugInteractionToolReportMapping.BuildNameWithAliases(
                        item.Name ?? string.Empty,
                        aliasNames));
            })
            .OrderBy(row => row.SupplementName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.SupplementId)
            .ToList();

        selectedTwice = DrugInteractionToolReportMapping.BuildSubsection(
            "info.supplements.selected_twice",
            DrugInteractionToolHeadlines.InfoSupplementsSelectedTwice,
            selectedTwiceRows);

        List<InfoSupplementRowReadModel> takenNowRows = regularSupplements
            .Where(row => row.Source == RegularSupplementSource.Database && row.Supplement is not null)
            .GroupBy(row => row.Supplement!.Id)
            .Select(group => group.First())
            .Select(row =>
            {
                IReadOnlyCollection<string> aliasNames = row.Supplement!.Aliases.ToList();
                string supplementName = row.Supplement.Name ?? string.Empty;

                return new InfoSupplementRowReadModel(
                    SupplementId: row.Supplement.Id,
                    SupplementName: supplementName,
                    AliasNames: aliasNames,
                    RegularIntake: DrugInteractionToolReportMapping.MapRegularIntakeSnapshot(row),
                    DisplayLine: DrugInteractionToolReportMapping.BuildNameWithAliases(supplementName, aliasNames));
            })
            .OrderBy(row => row.SupplementName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.SupplementId)
            .ToList();

        List<InfoOwnSupplementRowReadModel> ownSupplementRows = regularSupplements
            .Where(row => row.Source == RegularSupplementSource.Own && row.OwnSupplement is not null)
            .GroupBy(row => row.OwnSupplement!.Id)
            .Select(group => group.First())
            .Select(BuildOwnSupplementRow)
            .OrderBy(row => row.OwnSupplementName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.OwnSupplementId)
            .ToList();

        return new InfoSupplementsSectionReadModel(
            SectionCode: "info.supplements",
            TakenNow: DrugInteractionToolReportMapping.BuildSubsection(
                "info.supplements.taken_now",
                DrugInteractionToolHeadlines.InfoSupplementsTakenNow,
                takenNowRows),
            OwnSupplements: DrugInteractionToolReportMapping.BuildSubsection(
                "info.supplements.own_supplements",
                DrugInteractionToolHeadlines.InfoOwnSupplementsTakenNow,
                ownSupplementRows),
            Selected: selected,
            SelectedTwice: selectedTwice);
    }

    private static InfoOwnSupplementRowReadModel BuildOwnSupplementRow(ClientRegularSupplement regularSupplement)
    {
        ClientOwnSupplement ownSupplement = regularSupplement.OwnSupplement!;

        List<InfoOwnSupplementMemberReadModel> members = ownSupplement.Items
            .Where(item => item.Supplement is not null)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.SupplementId)
            .Select(item => new InfoOwnSupplementMemberReadModel(
                SupplementId: item.SupplementId,
                SupplementName: item.Supplement.Name ?? string.Empty,
                AliasNames: item.Supplement.Aliases.ToList()))
            .ToList();

        string memberSegment = string.Join(
            ", ",
            members.Select(member =>
                DrugInteractionToolReportMapping.BuildNameWithAliases(member.SupplementName, member.AliasNames)));

        return new InfoOwnSupplementRowReadModel(
            OwnSupplementId: ownSupplement.Id,
            OwnSupplementName: ownSupplement.Name,
            MemberSupplements: members,
            RegularIntake: DrugInteractionToolReportMapping.MapRegularIntakeSnapshot(regularSupplement),
            DisplayLine: members.Count == 0 ? ownSupplement.Name : $"{ownSupplement.Name}: {memberSegment}");
    }

    private static InfoFoodsSectionReadModel BuildFoodsSection(
        IReadOnlyCollection<ClientRegularFood> regularFoods,
        IReadOnlyCollection<Food> selectedFoods,
        bool includeUserData,
        ReportAudience audience)
    {
        string selectedHeadline = audience == ReportAudience.Provider
            ? DrugInteractionToolHeadlines.InfoFoodsSelectedPrescription
            : DrugInteractionToolHeadlines.InfoFoodsSelected;

        HashSet<Guid> regularFoodIds = includeUserData
            ? regularFoods.Where(row => row.Food is not null).Select(row => row.FoodId).ToHashSet()
            : [];

        ReportSubsectionReadModel<InfoSelectedItemRowReadModel> selected = BuildSelectedSubsection(
            "info.foods.selected",
            selectedHeadline,
            selectedFoods.Select(item => (
                item.Id,
                item.Name,
                (IReadOnlyCollection<string>)item.Aliases.ToList(),
                includeUserData ? (bool?)regularFoodIds.Contains(item.Id) : null)));

        if (!includeUserData)
        {
            return new InfoFoodsSectionReadModel(
                SectionCode: "info.foods",
                TakenNow: null,
                Selected: selected);
        }

        List<InfoFoodRowReadModel> takenNowRows = regularFoods
            .Where(row => row.Food is not null)
            .GroupBy(row => row.FoodId)
            .Select(group => group.First())
            .Select(row =>
            {
                IReadOnlyCollection<string> aliasNames = row.Food.Aliases.ToList();

                return new InfoFoodRowReadModel(
                    FoodId: row.FoodId,
                    FoodName: row.Food.Name,
                    AliasNames: aliasNames,
                    RegularIntake: DrugInteractionToolReportMapping.MapRegularIntakeSnapshot(row),
                    DisplayLine: DrugInteractionToolReportMapping.BuildNameWithAliases(row.Food.Name, aliasNames));
            })
            .OrderBy(row => row.FoodName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.FoodId)
            .ToList();

        return new InfoFoodsSectionReadModel(
            SectionCode: "info.foods",
            TakenNow: DrugInteractionToolReportMapping.BuildSubsection(
                "info.foods.taken_now",
                DrugInteractionToolHeadlines.InfoFoodsTakenNow,
                takenNowRows),
            Selected: selected);
    }
}
