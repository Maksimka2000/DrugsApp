using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using DrugsApp.Domain.RegularDrugsFeature.Shared;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Builds taken-now overlap rows from already-loaded regular drugs and database drug allergies [description].
/// </summary>
internal static class DrugTakenNowAllergyOverlapBuilder
{
    /// <summary>
    /// Builds the taken-now overlap subsection without additional database access.
    /// </summary>
    /// <param name="regularDrugs">Live regular drug rows already loaded for the user.</param>
    /// <param name="drugAllergies">Drug allergy rows already loaded for the user.</param>
    /// <returns>Always-present subsection whose row count may be zero.</returns>
    internal static ReportSubsectionReadModel<DrugTakenNowAllergyOverlapRowReadModel> BuildSubsection(
        IReadOnlyCollection<ClientRegularDrug> regularDrugs,
        IReadOnlyCollection<ClientDrugAllergy> drugAllergies)
    {
        IReadOnlyCollection<DrugTakenNowAllergyOverlapRowReadModel> rows = BuildRows(regularDrugs, drugAllergies);

        return DrugInteractionToolReportMapping.BuildSubsection(
            "allergies.drug.taken_now_overlap",
            DrugInteractionToolHeadlines.DrugAllergiesTakenNowOverlap,
            rows);
    }

    /// <summary>
    /// Builds the plan-taking overlap subsection for manually selected drugs without additional database access.
    /// </summary>
    /// <param name="plannedDrugs">Manually selected, eligible drug forms the user plans to take.</param>
    /// <param name="drugAllergies">Drug drug allergies rows already loaded for the user.</param>
    /// <returns>Always-present subsection whose row count may be zero.</returns>
    internal static ReportSubsectionReadModel<DrugTakenNowAllergyOverlapRowReadModel> BuildPlanTakingSubsection(
        IReadOnlyCollection<DrugFormIntake> plannedDrugs,
        IReadOnlyCollection<ClientDrugAllergy> drugAllergies)
    {
        IReadOnlyCollection<DrugTakenNowAllergyOverlapRowReadModel> rows = BuildRowsForIntakes(plannedDrugs, drugAllergies);

        return DrugInteractionToolReportMapping.BuildSubsection(
            "allergies.drug.plan_taking_overlap",
            DrugInteractionToolHeadlines.DrugAllergiesPlanTakingOverlap,
            rows);
    }

    /// <summary>
    /// Pairs each active eligible regular drug with each database drug allergy in memory.
    /// </summary>
    /// <param name="regularDrugs">Live regular drug rows already loaded for the user.</param>
    /// <param name="drugAllergies">Drug allergy rows already loaded for the user.</param>
    /// <returns>Deduped, sorted overlap rows.</returns>
    internal static IReadOnlyCollection<DrugTakenNowAllergyOverlapRowReadModel> BuildRows(
        IReadOnlyCollection<ClientRegularDrug> regularDrugs,
        IReadOnlyCollection<ClientDrugAllergy> drugAllergies)
    {
        List<DrugFormIntake> takenNowIntakes = regularDrugs
            .Where(row =>
                row.Status == RegularDrugStatus.Active
                && row.Drug is not null
                && row.DrugForm is not null
                && RegularDrugEligibility.IsEligibleDrug(row.Drug))
            .Select(row => new DrugFormIntake(row.Drug, row.DrugForm))
            .ToList();

        return BuildRowsForIntakes(takenNowIntakes, drugAllergies);
    }

    /// <summary>
    /// Pairs each intake with each database drug allergy in memory.
    /// </summary>
    /// <param name="intakes">Drug forms taken now or planned, already loaded with active substances.</param>
    /// <param name="drugAllergies">Drug drug allergies rows already loaded for the user.</param>
    /// <returns>Deduped, sorted overlap rows.</returns>
    internal static IReadOnlyCollection<DrugTakenNowAllergyOverlapRowReadModel> BuildRowsForIntakes(
        IReadOnlyCollection<DrugFormIntake> intakes,
        IReadOnlyCollection<ClientDrugAllergy> drugAllergies)
    {
        List<ClientDrugAllergy> databaseAllergies = drugAllergies
            .Where(allergy =>
                allergy.Source == AllergyRecordSource.Database
                && allergy.DrugForm?.Drug is not null)
            .ToList();

        List<DrugTakenNowAllergyOverlapRowReadModel> rows = new();
        HashSet<(Guid RegularFormId, Guid AllergyFormId)> sameDrugKeys = new();
        HashSet<(Guid RegularFormId, Guid AllergyFormId, Guid SubstanceId)> sharedSubstanceKeys = new();

        foreach (DrugFormIntake intake in intakes)
        {
            Drug regularDrug = intake.Drug;
            DrugForm regularForm = intake.Form;
            var regularSubstanceIds = regularDrug.ActiveSubstances
                .Select(substance => substance.Id)
                .ToHashSet();
            string regularSummaryText = DrugInteractionToolReportMapping.BuildDrugSummaryText(
                regularDrug,
                regularForm);

            foreach (ClientDrugAllergy allergy in databaseAllergies)
            {
                DrugForm allergyForm = allergy.DrugForm!;
                Drug allergyDrug = allergyForm.Drug;
                string allergySummaryText = DrugInteractionToolReportMapping.BuildDrugSummaryText(
                    allergyDrug,
                    allergyForm);

                if (regularDrug.Id == allergyDrug.Id)
                {
                    if (!sameDrugKeys.Add((regularForm.Id, allergyForm.Id)))
                    {
                        continue;
                    }

                    rows.Add(BuildSameDrugRow(
                        regularDrug.Id,
                        regularForm.Id,
                        regularSummaryText,
                        allergyDrug.Id,
                        allergyForm.Id,
                        allergySummaryText,
                        allergy.Note));
                    continue;
                }

                foreach (var substance in allergyDrug.ActiveSubstances
                    .Where(substance => regularSubstanceIds.Contains(substance.Id))
                    .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(substance => substance.Id))
                {
                    if (!sharedSubstanceKeys.Add((regularForm.Id, allergyForm.Id, substance.Id)))
                    {
                        continue;
                    }

                    rows.Add(BuildSharedSubstanceRow(
                        substance.Id,
                        substance.ActiveSubstanceName,
                        regularDrug.Id,
                        regularForm.Id,
                        regularSummaryText,
                        allergyDrug.Id,
                        allergyForm.Id,
                        allergySummaryText,
                        allergy.Note));
                }
            }
        }

        return rows
            .OrderBy(row => row.OverlapKind)
            .ThenBy(row => row.RegularDrugSummaryText, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.AllergyDrugSummaryText, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ActiveSubstanceName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static DrugTakenNowAllergyOverlapRowReadModel BuildSameDrugRow(
        Guid regularDrugId,
        Guid regularDrugFormId,
        string regularDrugSummaryText,
        Guid allergyDrugId,
        Guid allergyDrugFormId,
        string allergyDrugSummaryText,
        string? userNote)
    {
        return new DrugTakenNowAllergyOverlapRowReadModel(
            OverlapKind: DrugTakenNowAllergyOverlapKind.SameDrugName,
            RegularDrugId: regularDrugId,
            RegularDrugFormId: regularDrugFormId,
            RegularDrugSummaryText: regularDrugSummaryText,
            AllergyDrugId: allergyDrugId,
            AllergyDrugFormId: allergyDrugFormId,
            AllergyDrugSummaryText: allergyDrugSummaryText,
            UserNote: userNote,
            Title: DrugInteractionToolHeadlines.DrugTakenNowOverlapSameDrugName,
            DisplayLine: BuildDisplayLine(regularDrugSummaryText, allergyDrugSummaryText, userNote),
            ActiveSubstanceId: null,
            ActiveSubstanceName: null);
    }

    private static DrugTakenNowAllergyOverlapRowReadModel BuildSharedSubstanceRow(
        Guid activeSubstanceId,
        string activeSubstanceName,
        Guid regularDrugId,
        Guid regularDrugFormId,
        string regularDrugSummaryText,
        Guid allergyDrugId,
        Guid allergyDrugFormId,
        string allergyDrugSummaryText,
        string? userNote)
    {
        return new DrugTakenNowAllergyOverlapRowReadModel(
            OverlapKind: DrugTakenNowAllergyOverlapKind.SharedActiveSubstance,
            RegularDrugId: regularDrugId,
            RegularDrugFormId: regularDrugFormId,
            RegularDrugSummaryText: regularDrugSummaryText,
            AllergyDrugId: allergyDrugId,
            AllergyDrugFormId: allergyDrugFormId,
            AllergyDrugSummaryText: allergyDrugSummaryText,
            UserNote: userNote,
            Title: DrugInteractionToolHeadlines.DrugTakenNowOverlapSharedActiveSubstance,
            DisplayLine: BuildDisplayLine(
                regularDrugSummaryText,
                allergyDrugSummaryText,
                activeSubstanceName,
                userNote),
            ActiveSubstanceId: activeSubstanceId,
            ActiveSubstanceName: activeSubstanceName);
    }

    private static string BuildDisplayLine(params string?[] parts) =>
        string.Join(" | ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
}
