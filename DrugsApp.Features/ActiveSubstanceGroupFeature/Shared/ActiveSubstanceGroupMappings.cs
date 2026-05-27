using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using DrugsApp.Domain.InteractionFeature.Enums;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.Shared;

/// <summary>
/// Mapping helpers for active substance group projections.
/// </summary>
public static class ActiveSubstanceGroupMappings
{
    /// <summary>
    /// Maps an <see cref="ActiveSubstanceGroup"/> to its DTO representation.
    /// </summary>
    public static ActiveSubstanceGroupDto MapToDto(this ActiveSubstanceGroup group) => new(
        Id: group.Id,
        Name: group.Name,
        Members: group.ActiveSubstances.MapToDtoList(),
        Type: InteractionType.SubstanceGroup.ToString());

    /// <summary>
    /// Maps a collection of groups to DTO list.
    /// </summary>
    public static List<ActiveSubstanceGroupDto> MapToDtoList(this IEnumerable<ActiveSubstanceGroup> groups) =>
        groups.Select(group => group.MapToDto()).ToList();
}
