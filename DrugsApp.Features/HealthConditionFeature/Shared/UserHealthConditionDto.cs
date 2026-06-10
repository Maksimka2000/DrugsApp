using System;

namespace DrugsApp.Features.HealthConditionFeature.Shared;

/// <summary>
/// User health condition response model with resolved display name.
/// </summary>
public sealed record UserHealthConditionDto(
    Guid Id,
    string Source,
    Guid? HealthConditionId,
    string? CustomHealthConditionName,
    string DisplayName,
    string? Note,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    HealthConditionSummaryDto? HealthConditionSummary);
