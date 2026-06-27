namespace DrugsApp.Features.SupportFeature.Shared;

/// <summary>
/// Localized FAQ question and answer content.
/// </summary>
public sealed record FaqTranslationContentDto(
    string Question,
    string Answer);
