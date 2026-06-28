using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ActiveSubstanceFeature.DeleteActiveSubstance;

/// <summary>
/// Handles deletion of active substances with guards for groups, interactions, and drugs.
/// </summary>
public sealed class DeleteActiveSubstanceHandler(
    IActiveSubstanceRepository _activeSubstanceRepository,
    IActiveSubstanceGroupRepository _activeSubstanceGroupRepository,
    IInteractionRepository _interactionRepository,
    IDrugRepository _drugRepository)
    : IRequestHandler<DeleteActiveSubstanceCommand, ErrorOr<DeleteActiveSubstanceResponse>>
{
    /// <summary>
    /// Deletes an active substance when it is not used by groups, interactions, or drugs.
    /// </summary>
    public async Task<ErrorOr<DeleteActiveSubstanceResponse>> Handle(
        DeleteActiveSubstanceCommand command,
        CancellationToken cancellationToken)
    {
        ActiveSubstance? activeSubstance = await _activeSubstanceRepository.GetByIdAsync(
            command.ActiveSubstanceId,
            cancellationToken);

        if (activeSubstance is null)
        {
            return Error.NotFound(
                description: "Active substance not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "ActiveSubstance",
                    ["resourceId"] = command.ActiveSubstanceId.ToString()
                });
        }

        List<ActiveSubstanceGroup> activeSubstanceGroups =
            await _activeSubstanceGroupRepository.ListByMemberIdsAsync(
                [activeSubstance.Id],
                cancellationToken);

        if (activeSubstanceGroups.Count > 0)
        {
            return Error.Conflict(
                description: "Active substance cannot be removed because it is a member of active substance groups.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = activeSubstance.Id.ToString(),
                    ["groupIds"] = activeSubstanceGroups.Select(group => group.Id.ToString()).ToList(),
                    ["groupNames"] = activeSubstanceGroups.Select(group => group.Name).ToList()
                });
        }

        List<Interaction> interactions = await _interactionRepository.ListByActiveSubstanceIdAsync(
            activeSubstance.Id,
            cancellationToken);

        if (interactions.Count > 0)
        {
            return Error.Conflict(
                description: "Active substance cannot be removed because it participates in interactions.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = activeSubstance.Id.ToString(),
                    ["interactionIds"] = interactions.Select(interaction => interaction.Id.ToString()).ToList()
                });
        }

        List<Drug> referencingDrugs = await _drugRepository.ListByActiveSubstanceIdsAsync(
            [activeSubstance.Id],
            excludeDrugId: null,
            cancellationToken);

        if (referencingDrugs.Count > 0)
        {
            return Error.Conflict(
                description: "Active substance cannot be removed because it is included in drugs.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = activeSubstance.Id.ToString(),
                    ["drugIds"] = referencingDrugs.Select(drug => drug.Id.ToString()).ToList(),
                    ["drugNames"] = referencingDrugs.Select(drug => drug.CommercialName).ToList()
                });
        }

        _activeSubstanceRepository.Remove(activeSubstance);

        return activeSubstance.MapToDeletedResponse();
    }
}
