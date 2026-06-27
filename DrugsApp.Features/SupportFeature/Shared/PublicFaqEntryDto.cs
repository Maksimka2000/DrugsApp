using System;
using System.Text.Json.Serialization;

namespace DrugsApp.Features.SupportFeature.Shared;

/// <summary>
/// Public-facing FAQ entry payload for a single requested language.
/// </summary>
public sealed record PublicFaqEntryDto(
    Guid Id,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    FaqTranslationContentDto? En,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    FaqTranslationContentDto? Pl);
