using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Entities;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Selected drug form with the role it plays in the report, projected for duplication detection [description].
/// </summary>
/// <param name="Drug">Catalog drug with its active substances loaded.</param>
/// <param name="Form">Selected drug form of the catalog drug.</param>
/// <param name="Source">Whether the form was selected manually, taken regularly, or both.</param>
internal readonly record struct DuplicationSelection(Drug Drug, DrugForm Form, SelectedDrugSource? Source);

/// <summary>
/// Builds the four duplication subsections from already-loaded selections and substance groups [description].
/// </summary>
/// <remarks>
/// Each subsection answers one question and never repeats another one.
/// A drug form that is both selected and taken regularly is a drug form duplication.
/// Several forms of one drug are a drug duplication.
/// An active substance contained in different drugs is an exact active substance duplication.
/// An active substance group reached through different drugs is a group duplication.
/// </remarks>
internal static class DrugDuplicationBuilder
{
    /// <summary>
    /// Tells whether at least two different drugs are selected, which is the precondition for substance and group duplicates.
    /// </summary>
    /// <param name="selections">Selected drug forms in report order.</param>
    /// <returns>True when the selections contain two or more distinct drugs.</returns>
    internal static bool HasMultipleDrugs(IReadOnlyList<DuplicationSelection> selections) =>
        selections.Select(selection => selection.Drug.Id).Distinct().Skip(1).Any();

    /// <summary>
    /// Builds the duplication section without any database access.
    /// </summary>
    /// <param name="selections">Selected drug forms in report order.</param>
    /// <param name="substanceNameById">Active substance names keyed by identifier.</param>
    /// <param name="groups">Active substance groups that contain at least one selected substance.</param>
    /// <param name="regularSupplements">
    /// Active regular supplements with catalog supplement and own supplement members loaded; null or empty without user data.
    /// </param>
    /// <param name="selectedSupplements">Catalog supplements selected for the report; null or empty when none.</param>
    /// <param name="includeSupplementDuplication">False when food and supplement interactions are off; the subsection is then omitted.</param>
    /// <returns>Section with exact, group, drug, drug form and supplement subsections, each possibly empty.</returns>
    internal static DrugDuplicationSectionReadModel Build(
        IReadOnlyList<DuplicationSelection> selections,
        IReadOnlyDictionary<Guid, string> substanceNameById,
        IReadOnlyCollection<ActiveSubstanceGroup> groups,
        IReadOnlyCollection<ClientRegularSupplement>? regularSupplements = null,
        IReadOnlyCollection<Supplement>? selectedSupplements = null,
        bool includeSupplementDuplication = true)
    {
        List<SupplementDuplicationRowReadModel> supplementRows = SupplementDuplicationBuilder.BuildRows(
            regularSupplements ?? [],
            selectedSupplements);
        List<ExactDuplicationRowReadModel> exactRows = BuildExactRows(selections, substanceNameById);
        List<GroupDuplicationRowReadModel> groupRows = BuildGroupRows(selections, exactRows, groups);
        List<DrugDuplicationRowReadModel> drugRows = BuildDrugRows(selections);
        List<DrugFormDuplicationRowReadModel> drugFormRows = BuildDrugFormRows(selections);

        return new DrugDuplicationSectionReadModel(
            SectionCode: "drug_duplication",
            ExactActiveSubstanceDuplicates: DrugInteractionToolReportMapping.BuildSubsection(
                "drug_duplication.exact_active_substance",
                DrugInteractionToolHeadlines.ExactDuplication,
                exactRows),
            ActiveSubstanceGroupDuplicates: DrugInteractionToolReportMapping.BuildSubsection(
                "drug_duplication.active_substance_group",
                DrugInteractionToolHeadlines.GroupDuplication,
                groupRows),
            DrugDuplicates: DrugInteractionToolReportMapping.BuildSubsection(
                "drug_duplication.drug",
                DrugInteractionToolHeadlines.DrugDuplication,
                drugRows),
            DrugFormDuplicates: DrugInteractionToolReportMapping.BuildSubsection(
                "drug_duplication.drug_form",
                DrugInteractionToolHeadlines.DrugFormDuplication,
                drugFormRows),
            SupplementDuplication: includeSupplementDuplication
                ? DrugInteractionToolReportMapping.BuildSubsection(
                    "supplement_duplication.supplement",
                    DrugInteractionToolHeadlines.SupplementDuplication,
                    supplementRows)
                : null);
    }

    private static List<DrugFormDuplicationRowReadModel> BuildDrugFormRows(
        IReadOnlyList<DuplicationSelection> selections)
    {
        return selections
            .Where(selection => selection.Source == SelectedDrugSource.ManualAndUserRegular)
            .Select(selection =>
            {
                string summaryText = DrugInteractionToolReportMapping.BuildDuplicationSummaryText(
                    selection.Drug,
                    selection.Form);

                return new DrugFormDuplicationRowReadModel(
                    DrugId: selection.Drug.Id,
                    DrugFormId: selection.Form.Id,
                    DrugSummaryText: summaryText,
                    AffectedDrugs: MapSummaries(selection),
                    DisplayLine:
                    $"Drug form: {summaryText} IS SELECTED AND ALREADY TAKEN REGULARLY WHICH IS CONSIDERED AS A DUPLICATION.",
                    DuplicationKind: DuplicationKind.DrugForm);
            })
            .OrderBy(row => row.DrugSummaryText, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.DrugFormId)
            .ToList();
    }

    private static List<DrugDuplicationRowReadModel> BuildDrugRows(IReadOnlyList<DuplicationSelection> selections)
    {
        return selections
            .GroupBy(selection => selection.Drug.Id)
            .Where(group => group.Select(selection => selection.Form.Id).Distinct().Skip(1).Any())
            .Select(group =>
            {
                Drug drug = group.First().Drug;

                return new DrugDuplicationRowReadModel(
                    DrugId: drug.Id,
                    DrugName: drug.CommercialName,
                    AffectedDrugs: OrderSummaries(group.SelectMany(MapSummaries)),
                    DisplayLine:
                    $"Drug: {drug.CommercialName} IS SELECTED OR TAKEN IN MORE THAN ONE FORM WHICH IS CONSIDERED AS A DUPLICATION.",
                    DuplicationKind: DuplicationKind.Drug);
            })
            .OrderBy(row => row.DrugName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.DrugId)
            .ToList();
    }

    private static List<ExactDuplicationRowReadModel> BuildExactRows(
        IReadOnlyList<DuplicationSelection> selections,
        IReadOnlyDictionary<Guid, string> substanceNameById)
    {
        Dictionary<Guid, Dictionary<Guid, DuplicationSelection>> selectionsBySubstanceAndForm = new();

        foreach (DuplicationSelection selection in selections)
        {
            foreach (ActiveSubstance substance in selection.Drug.ActiveSubstances)
            {
                if (!selectionsBySubstanceAndForm.TryGetValue(substance.Id, out Dictionary<Guid, DuplicationSelection>? byForm))
                {
                    byForm = new Dictionary<Guid, DuplicationSelection>();
                    selectionsBySubstanceAndForm[substance.Id] = byForm;
                }

                byForm[selection.Form.Id] = selection;
            }
        }

        List<ExactDuplicationRowReadModel> rows = new();

        foreach ((Guid substanceId, Dictionary<Guid, DuplicationSelection> byForm) in selectionsBySubstanceAndForm
                     .Where(pair => CountDistinctDrugs(pair.Value.Values) > 1)
                     .OrderBy(pair => substanceNameById[pair.Key], StringComparer.OrdinalIgnoreCase))
        {
            string substanceName = substanceNameById[substanceId];

            rows.Add(new ExactDuplicationRowReadModel(
                ActiveSubstanceId: substanceId,
                ActiveSubstanceName: substanceName,
                AffectedDrugs: OrderSummaries(byForm.Values.SelectMany(MapSummaries)),
                DisplayLine:
                $"Active substance: {substanceName} IS IN BOTH DRUGS WHICH IS CONSIDERED AS A DUPLICATION.",
                DuplicationKind: DuplicationKind.ExactActiveSubstance));
        }

        return rows;
    }

    private static List<GroupDuplicationRowReadModel> BuildGroupRows(
        IReadOnlyList<DuplicationSelection> selections,
        IReadOnlyCollection<ExactDuplicationRowReadModel> exactRows,
        IReadOnlyCollection<ActiveSubstanceGroup> groups)
    {
        var exactSubstanceIds = exactRows.Select(row => row.ActiveSubstanceId).ToHashSet();
        List<GroupDuplicationRowReadModel> rows = new();

        foreach (ActiveSubstanceGroup group in groups.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ThenBy(g => g.Id))
        {
            var memberIds = group.ActiveSubstances.Select(member => member.Id).ToHashSet();
            List<DuplicationSelection> affectedSelections = selections
                .Where(selection => selection.Drug.ActiveSubstances.Any(substance => memberIds.Contains(substance.Id)))
                .GroupBy(selection => selection.Form.Id)
                .Select(formGroup => formGroup.First())
                .ToList();

            if (CountDistinctDrugs(affectedSelections) < 2)
            {
                continue;
            }

            bool onlyExactOverlap = affectedSelections.All(selection =>
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

            IReadOnlyCollection<AffectedActiveSubstanceReadModel> affectedActiveSubstances = selections
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
                AffectedDrugs: OrderSummaries(affectedSelections.SelectMany(MapSummaries)),
                DisplayLine:
                $"Active substance group: {group.Name} IS SHARED BETWEEN MULTIPLE SELECTED DRUGS.",
                DuplicationKind: DuplicationKind.ActiveSubstanceGroup));
        }

        return rows;
    }

    private static int CountDistinctDrugs(IEnumerable<DuplicationSelection> selections) =>
        selections.Select(selection => selection.Drug.Id).Distinct().Count();

    private static IReadOnlyList<DuplicationDrugSummaryReadModel> MapSummaries(DuplicationSelection selection) =>
        DrugInteractionToolReportMapping.MapDuplicationDrugSummaries(
            selection.Drug,
            selection.Form,
            selection.Source);

    private static IReadOnlyCollection<DuplicationDrugSummaryReadModel> OrderSummaries(
        IEnumerable<DuplicationDrugSummaryReadModel> summaries) =>
        summaries
            .OrderBy(summary => summary.OnlyDrugSummaryText, StringComparer.OrdinalIgnoreCase)
            .ThenBy(summary => summary.DrugId)
            .ThenBy(summary => summary.DrugFormId)
            .ThenBy(summary => summary.Source)
            .ToList();
}
