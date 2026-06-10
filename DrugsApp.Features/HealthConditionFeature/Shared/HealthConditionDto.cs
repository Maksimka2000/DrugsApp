using System;

namespace DrugsApp.Features.HealthConditionFeature.Shared;

/// <summary>
/// Catalog health condition response model.
/// </summary>
public sealed record HealthConditionDto(
    Guid Id,
    string HealthConditionName,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
