namespace DrugsApp.Features.SupplementFeature.Shared;

/// <summary>
/// Supplement DTO used across supplement feature endpoints.
/// </summary>
public sealed record SupplementDto(
    Guid Id,
    string Name,
    IReadOnlyCollection<string> Aliases);
