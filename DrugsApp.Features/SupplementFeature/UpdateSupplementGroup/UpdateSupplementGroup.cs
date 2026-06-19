using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.InteractionFeature.ApplicationServices;
using DrugsApp.Features.SupplementFeature.Shared;
using DrugsApp.Features.SupplementFeature.UpdateSupplementGroup;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupplementFeature.UpdateSupplementGroup;



/// <summary>
/// Handles updating supplement group name and membership.
/// </summary>
public sealed class UpdateSupplementGroupHandler(
    ISupplementGroupRepository _supplementGroupRepository,
    ISupplementRepository _supplementRepository,
    IInteractionExpandedSyncApplicationService _interactionExpandedSyncApplicationService)
    : IRequestHandler<UpdateSupplementGroupCommand, ErrorOr<UpdatedSupplementGroupResponse>>
{
    /// <summary>
    /// Handles the process of updating an existing supplement group, including its name and associated supplements.
    /// </summary>
    /// <param name="command">The command containing the supplement group ID and the details to update, such as the name and supplement IDs.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete, enabling task cancellation.</param>
    /// <returns>A response containing the updated supplement group details or an error if the process fails.</returns>
    public async Task<ErrorOr<UpdatedSupplementGroupResponse>> Handle(
        UpdateSupplementGroupCommand command,
        CancellationToken cancellationToken)
    {
        // load the supplement group from the repository
        SupplementGroup? group = await _supplementGroupRepository.GetByIdAsync(
            id: command.SupplementGroupId,
            cancellationToken: cancellationToken);
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

        // update the name if provided
        if (!string.IsNullOrWhiteSpace(command.Request.Name))
        {
            group.UpdateName(command.Request.Name);
        }

        var currentMemberIds = group.Supplements
            .Select(supplement => supplement.Id)
            .ToHashSet();

        // update the supplements if provided
        if (command.Request.SupplementIds is not null && command.Request.SupplementIds.Count > 0)
        {
            // convert the request supplement ids to a list
            var requestSupplementIds = command.Request.SupplementIds.ToList();

            // load the supplements from the repository
            List<Supplement> toAdd = await _supplementRepository.GetByIdsForLinkingAsync(ids: requestSupplementIds, cancellationToken: cancellationToken);

            // check if the supplements were found
            if (toAdd.Count != requestSupplementIds.Count)
            {
                var missingIds = requestSupplementIds.Except(toAdd.Select(x => x.Id)).ToList();
                
                    return Error.Validation(
                        description: "Some supplements were not found.",
                        metadata: new()
                        {
                            ["missingIds"] = string.Join(",", missingIds)
                        });
                
            }

            group.UpdateSupplements(toAdd);

            var requestedMemberIds = requestSupplementIds.ToHashSet();
            var addedMembers = requestedMemberIds.Except(currentMemberIds).ToList();
            var removedMembers = currentMemberIds.Except(requestedMemberIds).ToList();

            if (addedMembers.Count > 0 || removedMembers.Count > 0)
            {
                ErrorOr<Success> syncResult = await _interactionExpandedSyncApplicationService
                    .SyncAfterGroupMembershipChangedAsync(
                        new SyncInteractionExpandedAfterGroupMembershipChangedRequest(
                            InteractionType.SupplementGroup,
                            group.Id,
                            addedMembers,
                            removedMembers),
                        cancellationToken);

                if (syncResult.IsError)
                {
                    return syncResult.Errors;
                }
            }
        }

        return group.MapToResponse();
    }
}
