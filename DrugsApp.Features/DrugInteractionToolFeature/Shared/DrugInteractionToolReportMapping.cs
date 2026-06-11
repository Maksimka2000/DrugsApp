using System.Text;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Pure mapping helpers for drug interaction tool report display lines [description].
/// </summary>
internal static class DrugInteractionToolReportMapping
{
    internal static string BuildDrugDisplayLine(
        Drug drug,
        DrugForm form,
        bool showOralGuidance,
        IReadOnlyDictionary<Guid, ActiveSubstance> activeSubstanceById)
    {
        var parts = new List<string> { drug.CommercialName };

        string activeSubstanceSegment = string.Join(
            ", ",
            drug.ActiveSubstances
                .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(substance => substance.Id)
                .Select(substance =>
                {
                    if (!showOralGuidance || !form.IsOral)
                    {
                        return substance.ActiveSubstanceName;
                    }

                    if (!activeSubstanceById.TryGetValue(substance.Id, out ActiveSubstance loaded))
                    {
                        return substance.ActiveSubstanceName;
                    }

                    string? guidance = FormatOralGuidanceText(loaded.OralMedicationIntakeGuidelines);
                    return string.IsNullOrWhiteSpace(guidance)
                        ? substance.ActiveSubstanceName
                        : $"{substance.ActiveSubstanceName} ({guidance})";
                }));

        parts.Add(activeSubstanceSegment);

        string formulation = form.PharmaceuticalForms
            .OrderBy(pf => pf.FormulationName, StringComparer.OrdinalIgnoreCase)
            .Select(pf => pf.FormulationName)
            .FirstOrDefault() ?? string.Empty;
        parts.Add(formulation);
        parts.Add(form.Dosage ?? string.Empty);

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

    internal static ActiveSubstanceInDrugReadModel MapActiveSubstanceInDrug(
        ActiveSubstance substance,
        bool formIsOral,
        bool showOralGuidance,
        IReadOnlyDictionary<Guid, ActiveSubstance> guidanceSource)
    {
        string? oralGuidance = null;
        if (showOralGuidance && formIsOral && guidanceSource.TryGetValue(substance.Id, out ActiveSubstance loaded))
        {
            oralGuidance = FormatOralGuidanceText(loaded.OralMedicationIntakeGuidelines);
        }

        return new ActiveSubstanceInDrugReadModel(
            ActiveSubstanceId: substance.Id,
            Name: substance.ActiveSubstanceName,
            IsOralInSelectedForm: formIsOral,
            OralGuidanceText: oralGuidance);
    }

    internal static SelectedDrugSummaryReadModel MapSelectedDrugSummary(
        Drug drug,
        DrugForm form,
        bool showOralGuidance,
        IReadOnlyDictionary<Guid, ActiveSubstance> activeSubstanceById,
        SelectedDrugSource? source)
    {
        bool formIsOral = form.IsOral;
        IReadOnlyCollection<ActiveSubstanceInDrugReadModel> activeSubstances = drug.ActiveSubstances
            .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(substance => substance.Id)
            .Select(substance => MapActiveSubstanceInDrug(substance, formIsOral, showOralGuidance, activeSubstanceById))
            .ToList();

        string formulationName = form.PharmaceuticalForms
            .OrderBy(pf => pf.FormulationName, StringComparer.OrdinalIgnoreCase)
            .Select(pf => pf.FormulationName)
            .FirstOrDefault() ?? string.Empty;

        string? typeOfDrug = drug.TypeOfDrug is DrugType.Special or DrugType.Ophthalmic
            ? drug.TypeOfDrug.ToString()
            : null;

        string? typeOfProduct = drug.TypeOfProduct.Equals("Weterynaryjny", StringComparison.OrdinalIgnoreCase)
            ? drug.TypeOfProduct
            : null;

        return new SelectedDrugSummaryReadModel(
            DrugFormId: form.Id,
            DrugId: drug.Id,
            CommercialName: drug.CommercialName,
            ActiveSubstances: activeSubstances,
            FormulationName: formulationName,
            Dosage: form.Dosage ?? string.Empty,
            IsOral: formIsOral,
            TypeOfDrug: typeOfDrug,
            TypeOfProduct: typeOfProduct,
            Source: source,
            DisplayLine: BuildDrugDisplayLine(drug, form, showOralGuidance, activeSubstanceById));
    }

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
        string displayName,
        bool isTakenNow,
        string defaultText,
        string? stomachText,
        string severity)
    {
        string takenNowSuffix = isTakenNow ? " (user taking it now)" : string.Empty;
        string stomachSegment = string.IsNullOrWhiteSpace(stomachText) ? string.Empty : $" + {stomachText}";
        return $"{activeSubstanceName} => {displayName}{takenNowSuffix}: {defaultText}{stomachSegment} {severity}";
    }

    internal static string BuildComparedDrugGroupTitle(
        SelectedDrugSummaryReadModel currentDrug,
        SelectedDrugSummaryReadModel comparedDrug) =>
        $"({currentDrug.DisplayLine}) => ({comparedDrug.DisplayLine}) SEE DETAILS OF INTERACTION";

    internal static string BuildGlobalPairGroupTitle(
        SelectedDrugSummaryReadModel leftDrug,
        SelectedDrugSummaryReadModel rightDrug) =>
        $"{leftDrug.DisplayLine} => {rightDrug.DisplayLine}";

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
