using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;

namespace DrugsApp.Features.SupplementFeature.Shared;

/// <summary>
/// Mapping helpers for supplement feature DTOs.
/// </summary>
public static class SupplementMappings
{
    /// <summary>
    /// Maps a <see cref="Supplement"/> entity to a <see cref="SupplementDto"/> object.
    /// </summary>
    /// <param name="supplement">The <see cref="Supplement"/> entity to map.</param>
    /// <returns>A <see cref="SupplementDto"/> object representing the mapped data.</returns>
    public static SupplementDto MapToDto(this Supplement supplement)
    {
        return new SupplementDto(
            supplement.Id,
            supplement.Name,
            supplement.Aliases.ToList(),
            supplement.BuildSearchOutputText(),
            InteractionType.Supplement.ToString());
    }

    /// <summary>
    /// Maps a collection of <see cref="Supplement"/> entities to a list of <see cref="SupplementDto"/> objects.
    /// </summary>
    /// <param name="supplements">The collection of <see cref="Supplement"/> entities to map.</param>
    /// <returns>A list of <see cref="SupplementDto"/> objects representing the mapped data.</returns>
    public static List<SupplementDto> MapToDtoList(this IEnumerable<Supplement> supplements)
    {
        return supplements
            .Select(MapToDto)
            .ToList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    public static SupplementGroupDto MapToDto(this SupplementGroup group)
    {
        return new SupplementGroupDto(
            group.Id,
            group.Name,
            group.Supplements
                .Select(member => new SupplementMemberDto(member.Id, member.Name))
                .ToList(),
            InteractionType.SupplementGroup.ToString());
    }

    /// <summary>
    /// Maps a collection of <see cref="SupplementGroup"/> entities to a list of <see cref="SupplementGroupDto"/> objects.
    /// </summary>
    /// <param name="groups">The collection of <see cref="SupplementGroup"/> entities to map.</param>
    /// <returns>A list of <see cref="SupplementGroupDto"/> objects representing the mapped data.</returns>
    public static List<SupplementGroupDto> MapToDtoList(this IEnumerable<SupplementGroup> groups)
    {
        return groups
            .Select(MapToDto)
            .ToList();
    }
}
