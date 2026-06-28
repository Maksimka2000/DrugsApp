using System;
using System.Linq;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupplementFeature.DeleteSupplementGroup;



/// <summary>
/// Handles deletion of supplement groups with reference guards.
/// </summary>
public sealed class DeleteSupplementGroupHandler(
    ISupplementGroupRepository _supplementGroupRepository,
    IInteractionRepository _interactionRepository)
    : IRequestHandler<DeleteSupplementGroupCommand, ErrorOr<DeleteSupplementGroupResponse>>
{
    /// <summary>
    /// Handles the request to delete a supplement group.
    /// </summary>
    /// <param name="command">The command containing the identifier of the supplement group to be deleted.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response indicating the result of the deletion operation or an error if the operation fails.</returns>
    public async Task<ErrorOr<DeleteSupplementGroupResponse>> Handle(
        DeleteSupplementGroupCommand command,
        CancellationToken cancellationToken)
    {
        SupplementGroup? group = await _supplementGroupRepository.GetByIdAsync(
            command.SupplementGroupId,
            cancellationToken);

        if (group is null)
        {
            return Error.NotFound(
                description: "Supplement group not found.",
                metadata: new()
                {
                    ["resourceType"] = "SupplementGroup",
                    ["resourceId"] = command.SupplementGroupId.ToString()
                });
        }

        IReadOnlyCollection<Guid> interactionIds = await _interactionRepository.GetInteractionIdsByGroupPartyAsync(
            InteractionType.SupplementGroup,
            command.SupplementGroupId,
            cancellationToken);

        if (interactionIds.Count > 0)
        {
            return Error.Conflict(
                description: "Supplement group cannot be deleted because it participates in interactions.",
                metadata: new()
                {
                    ["supplementGroupId"] = command.SupplementGroupId.ToString(),
                    ["interactionIds"] = interactionIds.Select(id => id.ToString()).ToList()
                });
        }

        _supplementGroupRepository.Remove(group);

        return group.MapToDeletedResponse();
    }
}
