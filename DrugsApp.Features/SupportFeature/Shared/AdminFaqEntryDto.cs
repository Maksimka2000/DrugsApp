using System;

namespace DrugsApp.Features.SupportFeature.Shared;

/// <summary>
/// Admin-facing FAQ entry payload with audit fields.
/// </summary>
public sealed record AdminFaqEntryDto(
    Guid Id,
    string Question,
    string Answer,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
