using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugToolFeature.ApplicationServices;

namespace DrugsApp.Features.DrugToolFeature.Shared;

internal static class DrugToolMappings
{
    internal const string ThisDrugLabel = "THIS DRUG";
    internal const string AlcoholHeadline = "THIS DRUG IS INTERACTED WITH ALCOHOL SEE DETAILS";
    internal const string OralGuidanceHeadline =
        "RECOMMENDATIONS ON THE INTAKE TIME: THE DRUG YOU ARE TAKING HAS SOME RECOMMENDATION ON INTAKE TIME";

    internal static string MapScopeToApi(DrugToolInteractionScope scope) =>
        scope switch
        {
            DrugToolInteractionScope.UserOnly => "user_only",
            _ => "all"
        };

    internal static string MapSubsectionToApi(DrugToolInteractionSubsectionCode code) =>
        code switch
        {
            DrugToolInteractionSubsectionCode.UserMatches => "user_matches",
            _ => "catalog_remainder"
        };

    internal static GetDrugToolDetailResponse MapToResponse(DrugToolDetailBuildResult result) =>
        new(
            result.DrugId,
            result.CommercialName,
            result.TypeOfProduct,
            result.TypeOfDrug,
            result.SelectedDrugFormId,
            result.SelectedDrugFormSummary,
            result.DrugDisplayLine,
            result.ActiveSubstances,
            result.AvailableDrugForms,
            MapScopeToApi(result.InteractionScope),
            ["all", "user_only"],
            result.UserMealTimes,
            result.AlcoholSection,
            result.DrugSection,
            result.FoodSection,
            result.SupplementSection);

    internal static DrugToolFormSummaryDto MapFormSummary(DrugForm form) =>
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

    internal static string BuildDrugDisplayLine(Drug drug, DrugForm form)
    {
        string substanceNames = string.Join(
            ", ",
            drug.ActiveSubstances
                .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(substance => substance.Id)
                .Select(substance => substance.ActiveSubstanceName));

        string formulation = form.PharmaceuticalForms
            .OrderBy(pf => pf.FormulationName, StringComparer.OrdinalIgnoreCase)
            .Select(pf => pf.FormulationName)
            .FirstOrDefault() ?? string.Empty;

        return $"{drug.CommercialName} ({substanceNames}), {formulation}, {form.Dosage ?? string.Empty}";
    }

    internal static string BuildFormLabel(DrugForm form)
    {
        string formulation = form.PharmaceuticalForms
            .OrderBy(pf => pf.FormulationName, StringComparer.OrdinalIgnoreCase)
            .Select(pf => pf.FormulationName)
            .FirstOrDefault() ?? string.Empty;

        return $"{formulation} {form.Dosage ?? string.Empty}".Trim();
    }

    internal static string? FormatOralGuidanceText(OralMedicationIntakeGuidelines? guidelines)
    {
        if (guidelines is null)
        {
            return null;
        }

        List<string> segments =
        [
            $"food intake: {guidelines.FoodIntakeType}"
        ];

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

        return string.Join("; ", segments);
    }
}
