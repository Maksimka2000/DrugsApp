using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.SupportFeature.Entities;
using DrugsApp.Domain.SupportFeature.Enums;

namespace DrugsApp.Features.SupportFeature.Shared;

/// <summary>
/// Mapping helpers for support FAQ entities and DTOs.
/// </summary>
public static class FaqEntryMappings
{
    /// <summary>
    /// Maps a FAQ aggregate to the admin DTO with grouped translations.
    /// </summary>
    public static AdminFaqEntryDto MapToAdminDto(this FaqEntry faqEntry) =>
        faqEntry.TryMapToAdminDto()
        ?? throw new InvalidOperationException("English and Polish FAQ translations are required.");

    /// <summary>
    /// Maps a FAQ aggregate to the public DTO for the requested language.
    /// </summary>
    public static PublicFaqEntryDto MapToPublicDto(this FaqEntry faqEntry, FaqLanguageCode languageCode)
    {
        FaqEntryTranslation? translation = faqEntry.GetTranslation(languageCode);
        FaqTranslationContentDto? content = translation?.MapToContentDto();

        return languageCode switch
        {
            FaqLanguageCode.En => new PublicFaqEntryDto(
                Id: faqEntry.Id,
                En: content,
                Pl: null),
            FaqLanguageCode.Pl => new PublicFaqEntryDto(
                Id: faqEntry.Id,
                En: null,
                Pl: content),
            _ => throw new InvalidOperationException($"Unsupported FAQ language code '{languageCode}'.")
        };
    }

    /// <summary>
    /// Maps FAQ aggregates to a public DTO list for the requested language.
    /// </summary>
    public static List<PublicFaqEntryDto> MapToPublicDtoList(
        this IEnumerable<FaqEntry> faqEntries,
        FaqLanguageCode languageCode) =>
        faqEntries
            .Select(faqEntry => faqEntry.MapToPublicDto(languageCode))
            .ToList();

    /// <summary>
    /// Maps FAQ aggregates to an admin DTO list, skipping entries without both translations.
    /// </summary>
    public static List<AdminFaqEntryDto> MapToAdminDtoList(this IEnumerable<FaqEntry> faqEntries) =>
        faqEntries
            .Select(faqEntry => faqEntry.TryMapToAdminDto())
            .Where(faqEntry => faqEntry is not null)
            .Select(faqEntry => faqEntry!)
            .ToList();

    private static AdminFaqEntryDto? TryMapToAdminDto(this FaqEntry faqEntry)
    {
        FaqEntryTranslation? englishTranslation = faqEntry.GetTranslation(FaqLanguageCode.En);
        FaqEntryTranslation? polishTranslation = faqEntry.GetTranslation(FaqLanguageCode.Pl);

        if (englishTranslation is null || polishTranslation is null)
        {
            return null;
        }

        return new AdminFaqEntryDto(
            Id: faqEntry.Id,
            En: englishTranslation.MapToContentDto(),
            Pl: polishTranslation.MapToContentDto(),
            CreatedAtUtc: faqEntry.CreatedAtUtc,
            UpdatedAtUtc: faqEntry.UpdatedAtUtc);
    }

    private static FaqTranslationContentDto MapToContentDto(this FaqEntryTranslation translation) => new(
        Question: translation.Question,
        Answer: translation.Answer);
}
