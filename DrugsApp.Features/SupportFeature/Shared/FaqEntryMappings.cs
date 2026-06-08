using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.SupportFeature.Entities;

namespace DrugsApp.Features.SupportFeature.Shared;

/// <summary>
/// Mapping helpers for support FAQ entities and DTOs.
/// </summary>
public static class FaqEntryMappings
{
    /// <summary>
    /// Maps a FAQ aggregate to the public DTO.
    /// </summary>
    public static FaqEntryDto MapToDto(this FaqEntry faqEntry) => new(
        Id: faqEntry.Id,
        Question: faqEntry.Question,
        Answer: faqEntry.Answer);

    /// <summary>
    /// Maps a FAQ aggregate to the admin DTO.
    /// </summary>
    public static AdminFaqEntryDto MapToAdminDto(this FaqEntry faqEntry) => new(
        Id: faqEntry.Id,
        Question: faqEntry.Question,
        Answer: faqEntry.Answer,
        CreatedAtUtc: faqEntry.CreatedAtUtc,
        UpdatedAtUtc: faqEntry.UpdatedAtUtc);

    /// <summary>
    /// Maps FAQ aggregates to a public DTO list.
    /// </summary>
    public static List<FaqEntryDto> MapToDtoList(this IEnumerable<FaqEntry> faqEntries) =>
        faqEntries.Select(faqEntry => faqEntry.MapToDto()).ToList();

    /// <summary>
    /// Maps FAQ aggregates to an admin DTO list.
    /// </summary>
    public static List<AdminFaqEntryDto> MapToAdminDtoList(this IEnumerable<FaqEntry> faqEntries) =>
        faqEntries.Select(faqEntry => faqEntry.MapToAdminDto()).ToList();
}
