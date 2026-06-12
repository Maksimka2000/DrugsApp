using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Pure mapping helpers for drug interaction tool report display lines [description].
/// </summary>
internal static class DrugInteractionToolReportMapping
{
    internal static string BuildDrugSummaryText(Drug drug, DrugForm? form)
    {
        var parts = new List<string> { drug.CommercialName };

        string activeSubstanceSegment = string.Join(
            ", ",
            drug.ActiveSubstances
                .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(substance => substance.Id)
                .Select(substance => substance.ActiveSubstanceName));

        parts.Add(activeSubstanceSegment);

        if (form is not null)
        {
            string formulation = form.PharmaceuticalForms
                .OrderBy(pf => pf.FormulationName, StringComparer.OrdinalIgnoreCase)
                .Select(pf => pf.FormulationName)
                .FirstOrDefault() ?? string.Empty;
            parts.Add(formulation);
            parts.Add(form.Dosage ?? string.Empty);
        }

        if (drug.TypeOfDrug is DrugType.Special or DrugType.Ophthalmic)
        {
            parts.Add(drug.TypeOfDrug.ToString());
        }

        if (drug.TypeOfProduct.Equals("Weterynaryjny", StringComparison.OrdinalIgnoreCase))
        {
            parts.Add(drug.TypeOfProduct);
        }

        return string.Join(" | ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    internal static SelectedDrugFormSummaryReadModel MapFormSummary(DrugForm form) =>
        new(
            FormulationName: form.PharmaceuticalForms
                .OrderBy(pf => pf.FormulationName, StringComparer.OrdinalIgnoreCase)
                .Select(pf => pf.FormulationName)
                .FirstOrDefault() ?? string.Empty,
            Dosage: form.Dosage ?? string.Empty,
            IsOral: form.IsOral,
            RoutesOfAdministration: form.RoutesOfAdministration
                .OrderBy(route => route.RouteOfAdministrationName, StringComparer.OrdinalIgnoreCase)
                .Select(route => route.RouteOfAdministrationName)
                .ToList(),
            PharmaceuticalForms: form.PharmaceuticalForms
                .OrderBy(pf => pf.FormulationName, StringComparer.OrdinalIgnoreCase)
                .Select(pf => pf.FormulationName)
                .ToList(),
            Leaflet: form.Leaflet,
            EducationMaterials: form.EducationMaterials);

    internal static DrugTargetSummaryReadModel MapDrugTargetSummary(Drug drug, DrugForm form) =>
        new(
            DrugId: drug.Id,
            DrugFormId: form.Id,
            DrugSummaryText: BuildDrugSummaryText(drug, form),
            IsOral: form.IsOral);

    internal static DuplicationDrugSummaryReadModel MapDuplicationDrugSummary(Drug drug, DrugForm form) =>
        new(
            DrugId: drug.Id,
            OnlyDrugSummaryText: BuildDrugSummaryText(drug, form: null),
            IsOral: form.IsOral);

    internal static string BuildOtherCustomData(
        string? activeSubstances,
        string? dosage,
        string? formulationName)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(activeSubstances))
        {
            parts.Add(activeSubstances.Trim());
        }

        if (!string.IsNullOrWhiteSpace(dosage))
        {
            parts.Add(dosage.Trim());
        }

        if (!string.IsNullOrWhiteSpace(formulationName))
        {
            parts.Add(formulationName.Trim());
        }

        return string.Join(", ", parts);
    }

    internal static SpecificDrugDetailReadModel MapSpecificDrugDetail(Drug drug, DrugForm form) =>
        new(
            DrugId: drug.Id,
            CommercialName: drug.CommercialName,
            TypeOfProduct: drug.TypeOfProduct,
            TypeOfDrug: drug.TypeOfDrug,
            SelectedDrugFormId: form.Id,
            SelectedDrugFormSummary: MapFormSummary(form),
            ActiveSubstances: drug.ActiveSubstances
                .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(substance => substance.Id)
                .Select(substance => new ActiveSubstanceRefReadModel(substance.Id, substance.ActiveSubstanceName))
                .ToList());

    internal static string BuildSubstancePairDisplayLine(
        string currentName,
        string comparedName,
        string defaultText,
        string? stomachText,
        string severity)
    {
        string stomachSegment = string.IsNullOrWhiteSpace(stomachText) ? string.Empty : $" + {stomachText}";
        return $"{currentName} => {comparedName}: {defaultText}{stomachSegment} {severity}";
    }

    internal static string BuildFoodOrSupplementDisplayLine(
        string activeSubstanceName,
        string partyName,
        bool isTakenNow,
        string defaultText,
        string? stomachText,
        string severity)
    {
        string takenNowSuffix = isTakenNow ? " (user taking it now)" : string.Empty;
        string stomachSegment = string.IsNullOrWhiteSpace(stomachText) ? string.Empty : $" + {stomachText}";
        return $"{activeSubstanceName} => {partyName}{takenNowSuffix}: {defaultText}{stomachSegment} {severity}";
    }

    internal static string BuildComparedDrugGroupTitle(
        DrugTargetSummaryReadModel currentDrug,
        DrugTargetSummaryReadModel comparedDrug) =>
        $"({currentDrug.DrugSummaryText}) => ({comparedDrug.DrugSummaryText}) SEE DETAILS OF INTERACTION";

    internal static string BuildGlobalPairGroupTitle(
        DrugTargetSummaryReadModel leftDrug,
        DrugTargetSummaryReadModel rightDrug) =>
        $"{leftDrug.DrugSummaryText} => {rightDrug.DrugSummaryText}";

    internal static string FormatOralGuidanceText(OralMedicationIntakeGuidelines? guidelines)
    {
        if (guidelines is null)
        {
            return string.Empty;
        }

        var segments = new List<string>
        {
            $"food intake: {guidelines.FoodIntakeType}"
        };

        if (guidelines.WaitBeforeMinutes is not null)
        {
            segments.Add($"wait before meal: {guidelines.WaitBeforeMinutes} min");
        }

        if (guidelines.WaitAfterMinutes is not null)
        {
            segments.Add($"wait after meal: {guidelines.WaitAfterMinutes} min");
        }

        if (!string.IsNullOrWhiteSpace(guidelines.MealNotes))
        {
            segments.Add(guidelines.MealNotes.Trim());
        }

        if (!string.IsNullOrWhiteSpace(guidelines.AvoidFoodDrug))
        {
            segments.Add($"avoid with drug: {guidelines.AvoidFoodDrug.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(guidelines.AvoidFoodTherapy))
        {
            segments.Add($"avoid during therapy: {guidelines.AvoidFoodTherapy.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(guidelines.LiquidNotes))
        {
            segments.Add(guidelines.LiquidNotes.Trim());
        }

        if (guidelines.RecommendedMorning == true)
        {
            segments.Add("recommended in the morning");
        }

        if (guidelines.RecommendedEvening == true)
        {
            segments.Add("recommended in the evening");
        }

        return string.Join("; ", segments);
    }

    internal static ReportSubsectionReadModel<TRow> BuildSubsection<TRow>(
        string sectionCode,
        string headlineText,
        IReadOnlyCollection<TRow> rows) =>
        new(
            SectionCode: sectionCode,
            HeadlineText: headlineText,
            RecordCount: rows.Count,
            Rows: rows);
}
