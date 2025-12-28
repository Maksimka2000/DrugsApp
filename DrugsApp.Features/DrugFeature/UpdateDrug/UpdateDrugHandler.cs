using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugFeature.UpdateDrug;

/// <summary>
/// Handles partially updating an existing drug.
/// </summary>
internal sealed class UpdateDrugHandler : IRequestHandler<UpdateDrugCommand, ErrorOr<UpdatedDrugResponse>>
{
    private readonly IDrugRepository _drugRepository;
    private readonly IActiveSubstanceRepository _activeSubstanceRepository;

    public UpdateDrugHandler(
        IDrugRepository drugRepository,
        IActiveSubstanceRepository activeSubstanceRepository)
    {
        _drugRepository = drugRepository;
        _activeSubstanceRepository = activeSubstanceRepository;
    }

    /// <summary>
    /// Loads a drug, applies partial updates, and validates the active substance invariant.
    /// </summary>
    /// <param name="command">Update command with route id and request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated drug response.</returns>
    public async Task<ErrorOr<UpdatedDrugResponse>> Handle(
        UpdateDrugCommand command,
        CancellationToken cancellationToken)
    {
        Drug? drug = await _drugRepository.GetByIdAsync(command.DrugId, cancellationToken);
        if (drug is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "Drug",
                    ["resourceId"] = command.DrugId.ToString()
                });
        }

        if (!string.IsNullOrWhiteSpace(command.Request.CommercialName))
        {
            drug.UpdateCommercialName(command.Request.CommercialName);
        }

        if (command.Request.TypeOfDrug is not null)
        {
            drug.UpdateTypeOfDrug(command.Request.TypeOfDrug.Value);
        }

        if (command.Request.NewActiveSubstanceIds is not null)
        {
            var requestedIds = command.Request.NewActiveSubstanceIds
                .ToHashSet();

            var currentIds = drug.ActiveSubstances
                .Select(activeSubstance => activeSubstance.Id)
                .ToHashSet();

            var idsToAdd = requestedIds.Except(currentIds).ToList();
            var idsToRemove = currentIds.Except(requestedIds).ToList();

            var activeSubstancesToRemove = drug.ActiveSubstances.Where(activeSubstance => idsToRemove.Contains(activeSubstance.Id)).ToList();

            if (idsToAdd.Count > 0)
            {
                IReadOnlyCollection<ActiveSubstance> resolvedToAdd = await _activeSubstanceRepository.GetByIdsAsync(idsToAdd, cancellationToken);

                var resolvedIdSet = resolvedToAdd.Select(a => a.Id).ToHashSet();
                var missingIds = idsToAdd.Where(id => !resolvedIdSet.Contains(id)).ToList();
                if (missingIds.Count > 0)
                {
                    return Error.Validation(
                        description: "One or more active substances were not found.",
                        metadata: new()
                        {
                            ["missingActiveSubstanceIds"] = missingIds.Select(x => x.ToString()).ToList()
                        });
                }

                foreach (ActiveSubstance activeSubstance in resolvedToAdd)
                {
                    drug.AddActiveSubstance(activeSubstance);
                }
            }

            foreach (ActiveSubstance removeActiveSubstance in activeSubstancesToRemove)
            {
                drug.RemoveActiveSubstance(removeActiveSubstance);
            }

            if (drug.ActiveSubstances.Count == 0)
            {
                return Error.Validation(
                    description: "Drug must have at least one active substance.",
                    metadata: new()
                    {
                        ["error"] = "minActiveSubstances"
                    });
            }
        }

        return drug.MapToResponse();
    }
}

