using System.Globalization;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Entities;

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

    internal static string BuildDuplicationSummaryText(Drug drug, DrugForm form)
    {
        string activeSubstances = string.Join(
            ", ",
            drug.ActiveSubstances
                .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(substance => substance.Id)
                .Select(substance => substance.ActiveSubstanceName));

        string nameSegment = string.IsNullOrWhiteSpace(activeSubstances)
            ? drug.CommercialName
            : $"{drug.CommercialName} ({activeSubstances})";

        string formulation = form.PharmaceuticalForms
            .OrderBy(pf => pf.FormulationName, StringComparer.OrdinalIgnoreCase)
            .Select(pf => pf.FormulationName)
            .FirstOrDefault() ?? string.Empty;

        var parts = new List<string> { nameSegment, formulation, form.Dosage ?? string.Empty };

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

    internal static IReadOnlyList<DuplicationDrugSummaryReadModel> MapDuplicationDrugSummaries(
        Drug drug,
        DrugForm form,
        SelectedDrugSource? source)
    {
        List<DuplicationDrugSummaryReadModel> summaries = new();

        if (source is SelectedDrugSource.UserRegular or SelectedDrugSource.ManualAndUserRegular)
        {
            summaries.Add(BuildDuplicationDrugSummary(drug, form, DuplicationDrugSource.TakingNow));
        }

        if (source is not SelectedDrugSource.UserRegular)
        {
            summaries.Add(BuildDuplicationDrugSummary(drug, form, DuplicationDrugSource.Selected));
        }

        return summaries;
    }

    private static DuplicationDrugSummaryReadModel BuildDuplicationDrugSummary(
        Drug drug,
        DrugForm form,
        DuplicationDrugSource source) =>
        new(
            DrugId: drug.Id,
            DrugFormId: form.Id,
            OnlyDrugSummaryText: BuildDuplicationSummaryText(drug, form),
            IsOral: form.IsOral,
            Source: source);

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

    internal static SpecificDrugDetailReadModel MapSpecificDrugDetail(
        Drug drug,
        DrugForm form,
        SelectedDrugSource? source = null,
        RegularDrugIntakeReadModel? regularIntake = null) =>
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
                .ToList(),
            SelectedDrugSource: source,
            RegularIntake: MapRegularIntake(source, regularIntake));

    internal static RegularDrugIntakeReadModel? MapRegularIntake(
        SelectedDrugSource? source,
        ClientRegularDrug? regularDrug)
    {
        if (regularDrug is null)
        {
            return null;
        }

        return MapRegularIntake(source, MapRegularIntakeSnapshot(regularDrug));
    }

    internal static RegularDrugIntakeReadModel MapRegularIntakeSnapshot(ClientRegularDrug regularDrug)
    {
        List<string> times = regularDrug.IntakeTimes
            .Select(entry => entry.IntakeTime.ToString("HH:mm", CultureInfo.InvariantCulture))
            .OrderBy(time => time, StringComparer.Ordinal)
            .ToList();

        bool isOngoing = !regularDrug.StartDate.HasValue && !regularDrug.EndDate.HasValue;

        return new RegularDrugIntakeReadModel(
            IntakeTimes: times,
            StartDate: FormatIsoDate(regularDrug.StartDate),
            EndDate: FormatIsoDate(regularDrug.EndDate),
            IsOngoing: isOngoing);
    }

    private static RegularDrugIntakeReadModel? MapRegularIntake(
        SelectedDrugSource? source,
        RegularDrugIntakeReadModel? snapshot)
    {
        if (source is not SelectedDrugSource.UserRegular
            and not SelectedDrugSource.ManualAndUserRegular)
        {
            return null;
        }

        return snapshot;
    }

    private static string? FormatIsoDate(DateOnly? value) =>
        value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

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

    internal static string BuildNameWithAliases(string name, IReadOnlyCollection<string> aliasNames) =>
        aliasNames.Count == 0
            ? name
            : $"{name} ({string.Join(", ", aliasNames)})";

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

    internal static string FormatOralGuidanceText(OralMedicationIntakeGuidelines guidelines)
    {
        return string.Join(
            "; ",
            new[]
            {
                $"foodIntakeType: {(int)guidelines.FoodIntakeType}",
                $"waitBeforeMinutes: {guidelines.WaitBeforeMinutes ?? 0}",
                $"waitAfterMinutes: {guidelines.WaitAfterMinutes ?? 0}",
                $"mealNotes: {guidelines.MealNotes ?? string.Empty}",
                $"avoidFoodDrug: {guidelines.AvoidFoodDrug ?? string.Empty}",
                $"avoidFoodTherapy: {guidelines.AvoidFoodTherapy ?? string.Empty}",
                $"liquidNotes: {guidelines.LiquidNotes ?? string.Empty}",
                $"liquidInstructions: {(guidelines.LiquidInstructions ?? false).ToString().ToLowerInvariant()}",
                $"recommendedMorning: {(guidelines.RecommendedMorning ?? false).ToString().ToLowerInvariant()}",
                $"recommendedEvening: {(guidelines.RecommendedEvening ?? false).ToString().ToLowerInvariant()}",
            });
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
