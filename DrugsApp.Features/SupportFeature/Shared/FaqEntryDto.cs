using System;

namespace DrugsApp.Features.SupportFeature.Shared;

/// <summary>
/// Public-facing FAQ entry payload.
/// </summary>
public sealed record FaqEntryDto(
    Guid Id,
    string Question,
    string Answer);
