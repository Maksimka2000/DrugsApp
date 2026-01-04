using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugFeature.UpdateDrugForm;

/// <summary>
/// Handles partially updating a drug form owned by a specific drug.
/// </summary>
internal sealed class UpdateDrugFormHandler(
    IDrugFormRepository _drugFormRepository,
    IRouteOfAdministrationRepository _routeOfAdministrationRepository,
    IPharmaceuticalFormRepository _pharmaceuticalFormRepository) : IRequestHandler<UpdateDrugFormCommand, ErrorOr<UpdatedDrugFormResponse>>
{
    /// <summary>
    /// Loads a tracked drug form, validates ownership, applies updates, and returns the updated state.
    /// </summary>
    public async Task<ErrorOr<UpdatedDrugFormResponse>> Handle(
        UpdateDrugFormCommand command,
        CancellationToken cancellationToken)
    {
        DrugForm? drugForm = await _drugFormRepository.GetByIdAsync(command.DrugFormId, cancellationToken);
        if (drugForm is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "DrugForm",
                    ["resourceId"] = command.DrugFormId.ToString()
                });
        }

        if (drugForm.DrugId != command.DrugId)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "DrugForm",
                    ["resourceId"] = command.DrugFormId.ToString(),
                    ["drugId"] = command.DrugId.ToString()
                });
        }

        ApplyScalarUpdates(drugForm, command.Request);

        ErrorOr<bool> routesTouchedResult = await ApplyRoutesUpdateIfProvidedAsync(drugForm, command.Request, cancellationToken);
        if (routesTouchedResult.IsError)
        {
            return routesTouchedResult.Errors;
        }

        ErrorOr<bool> formsTouchedResult = await ApplyPharmaceuticalFormsUpdateIfProvidedAsync(drugForm, command.Request, cancellationToken);
        if (formsTouchedResult.IsError)
        {
            return formsTouchedResult.Errors;
        }

        bool routesTouched = routesTouchedResult.Value;

        if (routesTouched)
        {
            drugForm.RecalculateIsOral();
        }

        return drugForm.MapToResponse();
    }

    private static void ApplyScalarUpdates(DrugForm drugForm, UpdateDrugFormRequest request)
    {
        if (request.ClearAtcCode == true)
        {
            drugForm.ClearAtcCode();
        }
        else if (!string.IsNullOrWhiteSpace(request.AtcCode))
        {
            drugForm.SetAtcCode(request.AtcCode);
        }

        if (request.ClearDosage == true)
        {
            drugForm.ClearDosage();
        }
        else if (!string.IsNullOrWhiteSpace(request.Dosage))
        {
            drugForm.SetDosage(request.Dosage);
        }

        if (request.ClearLeaflet == true)
        {
            drugForm.ClearLeaflet();
        }
        else if (!string.IsNullOrWhiteSpace(request.Leaflet))
        {
            drugForm.SetLeaflet(request.Leaflet);
        }

        if (request.ClearEducationMaterials == true)
        {
            drugForm.ClearEducationMaterials();
        }
        else if (!string.IsNullOrWhiteSpace(request.EducationMaterials))
        {
            drugForm.SetEducationMaterials(request.EducationMaterials);
        }
    }

    private async Task<ErrorOr<bool>> ApplyRoutesUpdateIfProvidedAsync(
        DrugForm drugForm,
        UpdateDrugFormRequest request,
        CancellationToken cancellationToken)
    {
        if (request.RouteOfAdministrationIds is null)
        {
            return false;
        }

        var requestedIds = request.RouteOfAdministrationIds
            .ToHashSet();

        var currentIds = drugForm.RoutesOfAdministration
            .Select(route => route.Id)
            .ToHashSet();

        var idsToAdd = requestedIds.Except(currentIds).ToList();
        var idsToRemove = currentIds.Except(requestedIds).ToList();

        var routesToRemove = drugForm.RoutesOfAdministration.Where(route => idsToRemove.Contains(route.Id)).ToList();

        if (idsToAdd.Count > 0)
        {
            IReadOnlyCollection<RouteOfAdministration> resolvedToAdd = await _routeOfAdministrationRepository.GetByIdsAsync(idsToAdd, cancellationToken);

            var resolvedIdSet = resolvedToAdd.Select(r => r.Id).ToHashSet();
            var missingIds = idsToAdd.Where(id => !resolvedIdSet.Contains(id)).ToList();
            if (missingIds.Count > 0)
            {
                return Error.Validation(
                    description: "One or more routes of administration were not found.",
                    metadata: new()
                    {
                        ["missingRouteOfAdministrationIds"] = missingIds.Select(x => x.ToString()).ToList()
                    });
            }

            foreach (RouteOfAdministration route in resolvedToAdd)
            {
                drugForm.AddRouteOfAdministration(route);
            }
        }

        foreach (RouteOfAdministration removeRoute in routesToRemove)
        {
            drugForm.RemoveRouteOfAdministration(removeRoute);
        }

        return idsToAdd.Count > 0 || idsToRemove.Count > 0;
    }

    private async Task<ErrorOr<bool>> ApplyPharmaceuticalFormsUpdateIfProvidedAsync(
        DrugForm drugForm,
        UpdateDrugFormRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PharmaceuticalFormIds is null)
        {
            return false;
        }

        var requestedIds = request.PharmaceuticalFormIds
            .ToHashSet();

        var currentIds = drugForm.PharmaceuticalForms
            .Select(form => form.Id)
            .ToHashSet();

        var idsToAdd = requestedIds.Except(currentIds).ToList();
        var idsToRemove = currentIds.Except(requestedIds).ToList();

        var pharmaceuticalFormsToRemove = drugForm.PharmaceuticalForms.Where(form => idsToRemove.Contains(form.Id)).ToList();

        if (idsToAdd.Count > 0)
        {
            IReadOnlyCollection<PharmaceuticalForm> resolvedToAdd = await _pharmaceuticalFormRepository.GetByIdsAsync(
                ids: idsToAdd,
                cancellationToken: cancellationToken);

            var resolvedIdSet = resolvedToAdd.Select(f => f.Id).ToHashSet();
            var missingIds = idsToAdd.Where(id => !resolvedIdSet.Contains(id)).ToList();
            if (missingIds.Count > 0)
            {
                return Error.Validation(
                    description: "One or more pharmaceutical forms were not found.",
                    metadata: new()
                    {
                        ["missingPharmaceuticalFormIds"] = missingIds.Select(x => x.ToString()).ToList()
                    });
            }

            foreach (PharmaceuticalForm form in resolvedToAdd)
            {
                drugForm.AddPharmaceuticalForm(form);
            }
        }

        foreach (PharmaceuticalForm removeForm in pharmaceuticalFormsToRemove)
        {
            drugForm.RemovePharmaceuticalForm(removeForm);
        }

        return idsToAdd.Count > 0 || idsToRemove.Count > 0;
    }
}

