using System.Collections.Generic;
using DrugsApp.Features.SupportFeature.Shared;

namespace DrugsApp.Features.SupportFeature.SearchAdminFaqEntries;

/// <summary>
/// Mapping helpers for the admin FAQ search slice.
/// </summary>
public static class SearchAdminFaqEntriesMapping
{
    /// <summary>
    /// Maps the HTTP request to the MediatR query.
    /// </summary>
    public static SearchAdminFaqEntriesQuery MapToQuery(this SearchAdminFaqEntriesRequest request) => new(
        QuestionQuery: NormalizeQuestionQuery(request.QuestionQuery),
        Skip: request.Skip,
        Take: request.Take);

    /// <summary>
    /// Maps the response envelope for the admin FAQ list.
    /// </summary>
    public static SearchAdminFaqEntriesResponse MapToResponse(
        this IReadOnlyCollection<AdminFaqEntryDto> items,
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
