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
using MediatR;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.AddActiveSubstanceGroup;

/// <summary>
/// Handles creating active substance groups with uniqueness and membership checks.
/// </summary>
internal sealed class AddActiveSubstanceGroupHandler(
    IActiveSubstanceGroupRepository _activeSubstanceGroupRepository,
    IActiveSubstanceRepository _activeSubstanceRepository)
    : IRequestHandler<AddActiveSubstanceGroupCommand, ErrorOr<CreatedActiveSubstanceGroupResponse>>
{
    /// <summary>
    /// Creates a new active substance group after validation.
    /// </summary>
    /// <param name="command">Create group command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<ErrorOr<CreatedActiveSubstanceGroupResponse>> Handle(
        AddActiveSubstanceGroupCommand command,
        CancellationToken cancellationToken)
    {
        string normalizedName = command.Request.Name.Trim();

        ActiveSubstanceGroup? existing = await _activeSubstanceGroupRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existing is not null)
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

        IReadOnlyCollection<ActiveSubstance> membersResult = await _activeSubstanceRepository.GetByIdsForLinkingAsync(
            command.Request.ActiveSubstanceIds.ToList(),
            cancellationToken);
        var members = membersResult.ToList();

        var missingIds = command.Request.ActiveSubstanceIds
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
            command.Request.ActiveSubstanceIds,
            excludedGroupId: null,
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

        ActiveSubstanceGroup group = command.MapToEntity(members);

        await _activeSubstanceGroupRepository.AddAsync(group, cancellationToken);

        CreatedActiveSubstanceGroupResponse response = group.MapToResponse();

        return response;
    }
}
