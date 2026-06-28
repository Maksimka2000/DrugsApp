using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.DeleteActiveSubstanceGroup;

/// <summary>
/// Handles deleting active substance groups.
/// </summary>
internal sealed class DeleteActiveSubstanceGroupHandler(
    IActiveSubstanceGroupRepository _activeSubstanceGroupRepository,
    IInteractionRepository _interactionRepository)
    : IRequestHandler<DeleteActiveSubstanceGroupCommand, ErrorOr<DeleteActiveSubstanceGroupResponse>>
{
    /// <summary>
    /// Deletes a group when it exists.
    /// </summary>
    /// <param name="command">Delete command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<ErrorOr<DeleteActiveSubstanceGroupResponse>> Handle(
        DeleteActiveSubstanceGroupCommand command,
        CancellationToken cancellationToken)
    {
        ActiveSubstanceGroup? group = await _activeSubstanceGroupRepository.GetByIdAsync(
            command.GroupId,
            cancellationToken);

        if (group is null)
        {
            return Error.NotFound(
                description: "Active substance group not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = command.GroupId.ToString(),
                    ["resourceType"] = "ActiveSubstanceGroup"
                });
        }

        IReadOnlyCollection<Guid> interactionIds = await _interactionRepository.GetInteractionIdsByGroupPartyAsync(
            InteractionType.SubstanceGroup,
            command.GroupId,
            cancellationToken);

        if (interactionIds.Count > 0)
        {
            return Error.Conflict(
                description: "Active substance group cannot be deleted because it participates in interactions.",
                metadata: new Dictionary<string, object>
                {
                    ["activeSubstanceGroupId"] = command.GroupId.ToString(),
                    ["interactionIds"] = interactionIds.Select(id => id.ToString()).ToList()
                });
        }

        await _activeSubstanceGroupRepository.RemoveAsync(group, cancellationToken);

        return group.MapToDeletedResponse();
    }
}
