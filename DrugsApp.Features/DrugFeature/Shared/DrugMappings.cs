using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Features.DrugFeature.Shared;

/// <summary>
/// Mapping helpers for drug aggregates and DTO representations.
/// </summary>
internal static class DrugMappings
{
    public static DrugDto MapToDto(this Drug drug) => new(
        Id: drug.Id,
        CommercialName: drug.CommercialName,
        TypeOfProduct: drug.TypeOfProduct,
        TypeOfDrug: drug.TypeOfDrug,
        DrugFormsCount: drug.DrugFormCounter,
        ActiveSubstances: drug.ActiveSubstances
            .Select(activeSubstance => new LookupDto(activeSubstance.Id, activeSubstance.ActiveSubstanceName))
            .OrderBy(name => name.Name)
            .ToList(),
        DrugForms: drug.DrugForms
            .OrderBy(drugForm => drugForm.ProductIdentifier)
            .Select(drugForm => drugForm.MapToDto())
            .ToList());

    public static DrugFormDto MapToDto(this DrugForm drugForm) => new(
        Id: drugForm.Id,
        ProductIdentifier: drugForm.ProductIdentifier,
        AtcCode: drugForm.AtcCode,
        Dosage: drugForm.Dosage,
        Leaflet: drugForm.Leaflet,
        EducationMaterials: drugForm.EducationMaterials,
        IsOral: drugForm.IsOral,
        RoutesOfAdministration: drugForm.RoutesOfAdministration
            .Select(route => new LookupDto(route.Id, route.RouteOfAdministrationName))
            .OrderBy(x => x.Name)
            .ToList(),
        PharmaceuticalForms: drugForm.PharmaceuticalForms
            .Select(form => new LookupDto(form.Id, form.FormulationName))
            .OrderBy(x => x.Name)
            .ToList());

    public static List<DrugDto> MapToDtoList(this IEnumerable<Drug> drugs)
        => drugs.Select(drug => drug.MapToDto()).ToList();
}

