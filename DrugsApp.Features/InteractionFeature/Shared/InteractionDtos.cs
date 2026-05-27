using System;
using System.Collections.Generic;
using DrugsApp.Domain.InteractionFeature.Enums;

namespace DrugsApp.Features.InteractionFeature.Shared;

/// <summary>
/// Basic identifier + label pair.
/// </summary>
public sealed record IdLabelDto(Guid Id, string Label);

/// <summary>
/// Resolved party descriptor for responses.
/// </summary>
public sealed record InteractionQueryFoundDto(
    InteractionType Type,
    Guid Id,
    string Label,
    IReadOnlyCollection<IdLabelDto>? Members);

/// <summary>
/// Severity snapshot returned to clients.
/// </summary>
public sealed record InteractionSeverityDto(
    Guid Id,
    short SeverityNumber,
    string Name,
    string? Description);

/// <summary>
/// Interaction card returned in search responses.
/// </summary>
public sealed record InteractionCardDto(
    Guid InteractionId,
    InteractionQueryFoundDto Left,
    InteractionQueryFoundDto Right,
    InteractionSeverityDto Severity,
    string? DefaultText,
    string? StomachText,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
