using System;

namespace DrugsApp.Features.SupportFeature.Shared;

/// <summary>
/// Admin-facing FAQ entry payload with grouped translations and audit fields.
/// </summary>
public sealed record AdminFaqEntryDto(
    Guid Id,
    FaqTranslationContentDto En,
    FaqTranslationContentDto Pl,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
