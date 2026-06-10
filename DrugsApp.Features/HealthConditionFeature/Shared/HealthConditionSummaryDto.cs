namespace DrugsApp.Features.HealthConditionFeature.Shared;

/// <summary>
/// Lightweight catalog summary embedded inside user health condition responses.
/// </summary>
public sealed record HealthConditionSummaryDto(
    string HealthConditionName,
    string? Description);
