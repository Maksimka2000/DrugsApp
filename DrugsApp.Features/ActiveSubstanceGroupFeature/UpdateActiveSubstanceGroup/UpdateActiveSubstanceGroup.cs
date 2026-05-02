using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Features.ActiveSubstanceGroupFeature.Shared;
using ErrorOr;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using MediatR;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup;

/// <summary>
/// Handles updating active substance groups.
/// </summary>
internal sealed class UpdateActiveSubstanceGroupHandler(
    IActiveSubstanceGroupRepository _activeSubstanceGroupRepository,
    IActiveSubstanceRepository _activeSubstanceRepository)
    : IRequestHandler<UpdateActiveSubstanceGroupCommand, ErrorOr<UpdatedActiveSubstanceGroupResponse>>
{
    /// <summary>
    /// Updates name and membership set of a group.
    /// </summary>
    /// <param name="command">Update command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<ErrorOr<UpdatedActiveSubstanceGroupResponse>> Handle(
        UpdateActiveSubstanceGroupCommand command,
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

        if (!string.IsNullOrWhiteSpace(command.Request.Name))
        {
            string normalizedName = command.Request.Name.Trim();

            if (!group.Name.Equals(normalizedName))
            {
                ActiveSubstanceGroup? existing = await _activeSubstanceGroupRepository.GetByNameAsync(
                    normalizedName,
                    cancellationToken);

                if (existing is not null && existing.Id != group.Id)
                {
                    return Error.Conflict(
                        description: "An active substance group with the same name already exists.",
                        metadata: new Dictionary<string, object>
                        {
                            ["resourceId"] = existing.Id.ToString(),
                            ["resourceType"] = "ActiveSubstanceGroup",
                            ["conflictReason"] = "duplicateGroupName"
                        });
                }

                group.UpdateName(normalizedName);
            }
        }

        if (command.Request.ActiveSubstanceIds is not null)
        {
            IReadOnlyCollection<Guid> memberIds = command.Request.ActiveSubstanceIds.Distinct().ToList();

            IReadOnlyCollection<ActiveSubstance> membersResult = await _activeSubstanceRepository.GetByIdsAsync(
                memberIds,
                cancellationToken);
            var members = membersResult.ToList();

            var missingIds = memberIds
                .Where(id => members.All(member => member.Id != id))
                .ToList();

            if (missingIds.Count > 0)
            {
                return Error.Validation(
                    description: "One or more active substances were not found.",
                    metadata: new Dictionary<string, object>
                    {
                        ["missingActiveSubstanceIds"] = missingIds.Select(id => id.ToString()).ToArray()
                    });
            }

            bool duplicateMemberSetExists = await _activeSubstanceGroupRepository.ExistsWithExactMembersAsync(
                memberIds,
                excludedGroupId: group.Id,
                cancellationToken);

            if (duplicateMemberSetExists)
            {
                return Error.Conflict(
                    description: "A group with the same member set already exists.",
                    metadata: new Dictionary<string, object>
                    {
                        ["conflictReason"] = "duplicateMemberSet"
                    });
            }

            group.ReplaceMembers(members);
        }

        UpdatedActiveSubstanceGroupResponse response = group.MapToResponse();

        return response;
    }
}
