using System;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugFeature.Shared;

namespace DrugsApp.Features.DrugFeature.UpdateDrugForm;

internal static class UpdateDrugFormMapping
{
    public static UpdateDrugFormCommand MapToCommand(
        this UpdateDrugFormRequest request,
        Guid drugId,
        Guid drugFormId) => new(
        DrugId: drugId,
        DrugFormId: drugFormId,
        Request: request);

    public static UpdatedDrugFormResponse MapToResponse(this DrugForm drugForm) => new(
        DrugId: drugForm.DrugId,
        DrugFormId: drugForm.Id,
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
}

