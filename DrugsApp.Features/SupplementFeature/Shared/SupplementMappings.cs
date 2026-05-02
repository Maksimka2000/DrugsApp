using DrugsApp.Domain.SupplementFeature.Entities;

namespace DrugsApp.Features.SupplementFeature.Shared;

/// <summary>
/// Mapping helpers for supplement feature DTOs.
/// </summary>
public static class SupplementMappings
{
    public static SupplementDto MapToDto(this Supplement supplement)
    {
        return new SupplementDto(
            supplement.Id,
            supplement.Name,
            supplement.Aliases.ToList());
    }

    public static List<SupplementDto> MapToDtoList(this IEnumerable<Supplement> supplements)
    {
        return supplements
            .Select(MapToDto)
            .ToList();
    }

    public static SupplementGroupDto MapToDto(this SupplementGroup group)
    {
        return new SupplementGroupDto(
            group.Id,
            group.Name,
            group.Supplements
                .Select(member => new SupplementMemberDto(member.Id, member.Name))
                .ToList());
    }

    public static List<SupplementGroupDto> MapToDtoList(this IEnumerable<SupplementGroup> groups)
    {
        return groups
            .Select(MapToDto)
            .ToList();
    }
}
