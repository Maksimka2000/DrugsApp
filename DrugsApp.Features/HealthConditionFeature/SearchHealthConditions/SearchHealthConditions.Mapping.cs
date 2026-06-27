using System.Collections.Generic;
using System.Linq;
using DrugsApp.Features.HealthConditionFeature.Shared;

namespace DrugsApp.Features.HealthConditionFeature.SearchHealthConditions;

/// <summary>
/// Mapping helpers for search health conditions use case.
/// </summary>
public static class SearchHealthConditionsMapping
{
    /// <summary>
    /// Creates a query for searching health conditions with normalized name, paging options.
    /// </summary>
    /// <param name="request">Search request containing name query and paging arguments.</param>
    /// <returns>Query ready for search handler.</returns>
    public static SearchHealthConditionsQuery MapToQuery(this SearchHealthConditionsRequest request)
    {
        return new SearchHealthConditionsQuery(
            HealthConditionNameQuery: NormalizeQuery(request.HealthConditionNameQuery),
            Skip: request.Skip,
            Take: request.Take);
    }

    /// <summary>
    /// Projects search results into a response with paging metadata.
    /// </summary>
    /// <param name="items">List of result items.</param>
    /// <param name="totalCount">Optional total count for pagination.</param>
    /// <param name="hasMore">Indicates if more items are available.</param>
    /// <returns>Response DTO for search results.</returns>
    public static SearchHealthConditionsResponse MapToResponse(
        this IReadOnlyCollection<HealthConditionDto> items,
        int? totalCount,
        bool hasMore)
    {
        return new SearchHealthConditionsResponse(
            Items: items.ToList(),
            TotalCount: totalCount,
            HasMore: hasMore);
    }

    private static string? NormalizeQuery(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return string.Join(
            ' ',
            trimmed.Split((char[]?)null, System.StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
    }
}
