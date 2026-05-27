namespace DrugsApp.Features.SupplementFeature.Shared;

/// <summary>
/// Supplement group DTO with member summary.
/// </summary>
public sealed record SupplementGroupDto(
    Guid Id,
    string Name,
    IReadOnlyCollection<SupplementMemberDto> Supplements,
    string Type);

/// <summary>
/// Summary representation of a supplement member inside a group.
/// </summary>
public sealed record SupplementMemberDto(
    Guid Id,
    string Name);
