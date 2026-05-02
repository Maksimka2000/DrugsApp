using System;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Features.ActiveSubstanceGroupFeature.Shared;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup;

/// <summary>
/// Mapping helpers for UpdateActiveSubstanceGroup use case.
/// </summary>
public static class UpdateActiveSubstanceGroupMapping
{
    /// <summary>
    /// Maps the request to command with route id.
    /// </summary>
    public static UpdateActiveSubstanceGroupCommand MapToCommand(
        this UpdateActiveSubstanceGroupRequest request,
        Guid id) => new(id, request);

    /// <summary>
    /// Maps aggregate to updated response.
    /// </summary>
    public static UpdatedActiveSubstanceGroupResponse MapToResponse(this ActiveSubstanceGroup group) => new(
        Id: group.Id,
        Name: group.Name,
        Members: group.ActiveSubstances.MapToDtoList());
}
