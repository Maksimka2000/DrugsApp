using System.Collections.Generic;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Features.ActiveSubstanceGroupFeature.Shared;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.AddActiveSubstanceGroup;

/// <summary>
/// Mapping helpers for AddActiveSubstanceGroup use case.
/// </summary>
public static class AddActiveSubstanceGroupMapping
{
    /// <summary>
    /// Maps the request to command.
    /// </summary>
    public static AddActiveSubstanceGroupCommand MapToCommand(this AddActiveSubstanceGroupRequest request) => new(request);

    /// <summary>
    /// Maps command to domain entity using resolved members.
    /// </summary>
    public static ActiveSubstanceGroup MapToEntity(
        this AddActiveSubstanceGroupCommand command,
        IReadOnlyCollection<ActiveSubstance> members)
    {
        return ActiveSubstanceGroup.Create(
            command.Request.Name,
            members);
    }

    /// <summary>
    /// Maps aggregate to created response.
    /// </summary>
    public static CreatedActiveSubstanceGroupResponse MapToResponse(this ActiveSubstanceGroup group) => new(
        Id: group.Id,
        Name: group.Name,
        Members: group.ActiveSubstances.MapToDtoList());
}
