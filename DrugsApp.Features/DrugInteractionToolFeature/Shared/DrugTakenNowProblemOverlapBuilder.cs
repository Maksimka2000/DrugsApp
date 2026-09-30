using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Enums;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using DrugsApp.Domain.RegularDrugsFeature.Shared;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Builds taken-now overlap rows from already-loaded regular drugs and database drug problems [description].
/// </summary>
internal static class DrugTakenNowProblemOverlapBuilder
{
    /// <summary>
    /// Builds the taken-now overlap subsection without additional database access.
    /// </summary>
    /// <param name="regularDrugs">Live regular drug rows already loaded for the user.</param>
    /// <param name="drugProblems">Drug problem rows already loaded for the user.</param>
    /// <returns>Always-present subsection whose row count may be zero.</returns>
    internal static ReportSubsectionReadModel<DrugTakenNowProblemOverlapRowReadModel> BuildSubsection(
        IReadOnlyCollection<ClientRegularDrug> regularDrugs,
        IReadOnlyCollection<DrugProblem> drugProblems)
    {
        IReadOnlyCollection<DrugTakenNowProblemOverlapRowReadModel> rows = BuildRows(regularDrugs, drugProblems);

        return DrugInteractionToolReportMapping.BuildSubsection(
            "problematic_drugs.taken_now_overlap",
            DrugInteractionToolHeadlines.ProblematicDrugsTakenNowOverlap,
            rows);
    }

    /// <summary>
    /// Pairs each active eligible regular drug with each database drug problem in memory.
    /// </summary>
    /// <param name="regularDrugs">Live regular drug rows already loaded for the user.</param>
    /// <param name="drugProblems">Drug problem rows already loaded for the user.</param>
    /// <returns>Deduped, sorted overlap rows.</returns>
    internal static IReadOnlyCollection<DrugTakenNowProblemOverlapRowReadModel> BuildRows(
        IReadOnlyCollection<ClientRegularDrug> regularDrugs,
        IReadOnlyCollection<DrugProblem> drugProblems)
    {
        List<ClientRegularDrug> takenNowRows = regularDrugs
            .Where(row =>
                row.Status == RegularDrugStatus.Active
                && row.Drug is not null
                && row.DrugForm is not null
                && RegularDrugEligibility.IsEligibleDrug(row.Drug))
            .ToList();

        List<DrugProblem> databaseProblems = drugProblems
            .Where(problem =>
                problem.Source == DrugProblemSource.Database
                && problem.DrugForm?.Drug is not null)
            .ToList();

        List<DrugTakenNowProblemOverlapRowReadModel> rows = new();
        HashSet<(Guid RegularFormId, Guid ProblemFormId)> sameDrugKeys = new();
        HashSet<(Guid RegularFormId, Guid ProblemFormId, Guid SubstanceId)> sharedSubstanceKeys = new();

        foreach (ClientRegularDrug regularRow in takenNowRows)
        {
            Drug regularDrug = regularRow.Drug;
            DrugForm regularForm = regularRow.DrugForm;
            var regularSubstanceIds = regularDrug.ActiveSubstances
                .Select(substance => substance.Id)
                .ToHashSet();
            string regularSummaryText = DrugInteractionToolReportMapping.BuildDrugSummaryText(
                regularDrug,
                regularForm);

            foreach (DrugProblem problem in databaseProblems)
            {
                DrugForm problemForm = problem.DrugForm!;
                Drug problemDrug = problemForm.Drug;
                string problemSummaryText = DrugInteractionToolReportMapping.BuildDrugSummaryText(
                    problemDrug,
                    problemForm);

                if (regularDrug.Id == problemDrug.Id)
                {
                    if (!sameDrugKeys.Add((regularForm.Id, problemForm.Id)))
                    {
                        continue;
                    }

                    rows.Add(BuildSameDrugRow(
                        regularDrug.Id,
                        regularForm.Id,
                        regularSummaryText,
                        problemDrug.Id,
                        problemForm.Id,
                        problemSummaryText,
                        problem.ProblemNote));
                    continue;
                }

                foreach (var substance in problemDrug.ActiveSubstances
                    .Where(substance => regularSubstanceIds.Contains(substance.Id))
                    .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(substance => substance.Id))
                {
                    if (!sharedSubstanceKeys.Add((regularForm.Id, problemForm.Id, substance.Id)))
                    {
                        continue;
                    }

                    rows.Add(BuildSharedSubstanceRow(
                        substance.Id,
                        substance.ActiveSubstanceName,
                        regularDrug.Id,
                        regularForm.Id,
                        regularSummaryText,
                        problemDrug.Id,
                        problemForm.Id,
                        problemSummaryText,
                        problem.ProblemNote));
                }
            }
        }

        return rows
            .OrderBy(row => row.OverlapKind)
            .ThenBy(row => row.RegularDrugSummaryText, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ProblemDrugSummaryText, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ActiveSubstanceName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static DrugTakenNowProblemOverlapRowReadModel BuildSameDrugRow(
        Guid regularDrugId,
        Guid regularDrugFormId,
        string regularDrugSummaryText,
        Guid problemDrugId,
        Guid problemDrugFormId,
        string problemDrugSummaryText,
        string? userNote)
    {
        return new DrugTakenNowProblemOverlapRowReadModel(
            OverlapKind: DrugTakenNowAllergyOverlapKind.SameDrugName,
            RegularDrugId: regularDrugId,
            RegularDrugFormId: regularDrugFormId,
            RegularDrugSummaryText: regularDrugSummaryText,
            ProblemDrugId: problemDrugId,
            ProblemDrugFormId: problemDrugFormId,
            ProblemDrugSummaryText: problemDrugSummaryText,
            UserNote: userNote,
            Title: DrugInteractionToolHeadlines.DrugTakenNowOverlapSameDrugName,
            DisplayLine: BuildDisplayLine(regularDrugSummaryText, problemDrugSummaryText, userNote),
            ActiveSubstanceId: null,
            ActiveSubstanceName: null);
    }

    private static DrugTakenNowProblemOverlapRowReadModel BuildSharedSubstanceRow(
        Guid activeSubstanceId,
        string activeSubstanceName,
        Guid regularDrugId,
        Guid regularDrugFormId,
        string regularDrugSummaryText,
        Guid problemDrugId,
        Guid problemDrugFormId,
        string problemDrugSummaryText,
        string? userNote)
    {
        return new DrugTakenNowProblemOverlapRowReadModel(
            OverlapKind: DrugTakenNowAllergyOverlapKind.SharedActiveSubstance,
            RegularDrugId: regularDrugId,
            RegularDrugFormId: regularDrugFormId,
            RegularDrugSummaryText: regularDrugSummaryText,
            ProblemDrugId: problemDrugId,
            ProblemDrugFormId: problemDrugFormId,
            ProblemDrugSummaryText: problemDrugSummaryText,
            UserNote: userNote,
            Title: DrugInteractionToolHeadlines.DrugTakenNowOverlapSharedActiveSubstance,
            DisplayLine: BuildDisplayLine(
                regularDrugSummaryText,
                problemDrugSummaryText,
                activeSubstanceName,
                userNote),
            ActiveSubstanceId: activeSubstanceId,
            ActiveSubstanceName: activeSubstanceName);
    }

    private static string BuildDisplayLine(params string?[] parts) =>
        string.Join(" | ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
}
