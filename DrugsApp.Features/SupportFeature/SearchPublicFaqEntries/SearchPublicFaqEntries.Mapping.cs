using System.Collections.Generic;
using DrugsApp.Features.SupportFeature.Shared;

namespace DrugsApp.Features.SupportFeature.SearchPublicFaqEntries;

/// <summary>
/// Mapping helpers for the public FAQ search slice.
/// </summary>
public static class SearchPublicFaqEntriesMapping
{
    /// <summary>
    /// Maps the HTTP request to the MediatR query.
    /// </summary>
    public static SearchPublicFaqEntriesQuery MapToQuery(this SearchPublicFaqEntriesRequest request) => new(
        QuestionQuery: NormalizeQuestionQuery(request.QuestionQuery),
        Skip: request.Skip,
        Take: request.Take);

    /// <summary>
    /// Maps the response envelope for the public FAQ list.
    /// </summary>
    public static SearchPublicFaqEntriesResponse MapToResponse(
        this IReadOnlyCollection<FaqEntryDto> items,
        int? totalCount,
        bool hasMore) => new(
        Items: items,
        TotalCount: totalCount,
        HasMore: hasMore);

    private static string? NormalizeQuestionQuery(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
