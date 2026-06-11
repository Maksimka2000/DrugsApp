using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugToolFeature.ApplicationServices;

namespace DrugsApp.Features.DrugToolFeature.Shared;

internal static class DrugToolMappings
{
    internal static string MapScopeToApi(DrugToolInteractionScope scope) =>
        scope switch
        {
            DrugToolInteractionScope.UserOnly => "user_only",
            _ => "all"
        };

    internal static string MapSubsectionToApi(DrugToolInteractionSubsectionCode code) =>
        code switch
        {
            DrugToolInteractionSubsectionCode.UserData => "userdata",
            DrugToolInteractionSubsectionCode.AllOther => "all other",
            _ => "all"
        };

    internal static GetDrugToolDetailResponse MapToResponse(DrugToolDetailBuildResult result) =>
        new(
            result.DrugId,
            result.CommercialName,
            result.TypeOfProduct,
            result.TypeOfDrug,
            result.SelectedDrugFormId,
            result.SelectedDrugFormSummary,
            result.ActiveSubstances,
            MapScopeToApi(result.InteractionScope),
            ["all", "user_only"],
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

    internal static string BuildFormLabel(DrugForm form)
    {
        string formulation = form.PharmaceuticalForms
            .OrderBy(pf => pf.FormulationName, StringComparer.OrdinalIgnoreCase)
            .Select(pf => pf.FormulationName)
            .FirstOrDefault() ?? string.Empty;

        return $"{formulation} {form.Dosage ?? string.Empty}".Trim();
    }

    internal static string BuildDrugSummaryText(Drug drug, DrugForm form)
    {
        var parts = new List<string> { drug.CommercialName };

        string activeSubstanceSegment = string.Join(
            ", ",
            drug.ActiveSubstances
                .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(substance => substance.Id)
                .Select(substance => substance.ActiveSubstanceName));

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

    internal static IReadOnlyCollection<T>? WrapSubsection<T>(T? subsection) =>
        subsection is null ? null : [subsection];
}
