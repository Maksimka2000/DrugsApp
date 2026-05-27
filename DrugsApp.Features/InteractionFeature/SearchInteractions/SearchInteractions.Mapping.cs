using System;
using DrugsApp.Domain.InteractionFeature.Enums;

namespace DrugsApp.Features.InteractionFeature.SearchInteractions;

/// <summary>
/// Provides mapping extensions to transform a <see cref="SearchInteractionsRequest"/>
/// into a <see cref="SearchInteractionsQuery"/> to facilitate query processing
/// for search interactions functionality.
/// </summary>
public static class SearchInteractionsMapping
{
    /// <summary>
    /// Maps a <see cref="SearchInteractionsRequest"/> to a <see cref="SearchInteractionsQuery"/> object.
    /// </summary>
    /// <param name="request">
    /// The request containing interaction search parameters, such as interaction kind, query terms for each entity, sorting options, and pagination details.
    /// </param>
    /// <returns>
    /// A <see cref="SearchInteractionsQuery"/> object with the mapped interaction search parameters used for querying the system.
    /// </returns>
    public static SearchInteractionsQuery MapToQuery(this SearchInteractionsRequest request)
    {
        return new SearchInteractionsQuery(
            InteractionKind: MapInteractionKind(request.InteractionKind),
            LeftQuery: NormalizeQuery(request.LeftQuery),
            RightQuery: NormalizeQuery(request.RightQuery),
            SortBy: MapSortBy(request.SortBy),
            Ascending: request.OrderBy.Equals("ascending", StringComparison.OrdinalIgnoreCase),
            Skip: request.Skip,
            Take: request.Take);
    }

    private static string? NormalizeQuery(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static InteractionKind MapInteractionKind(string interactionKind)
    {
        return interactionKind.ToLowerInvariant() switch
        {
            "food" => InteractionKind.Food,
            "supplement" => InteractionKind.Supplement,
            _ => InteractionKind.Substance
        };
    }

    private static InteractionSortBy MapSortBy(string sortBy)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "severity" => InteractionSortBy.Severity,
            "created_at" => InteractionSortBy.CreatedAt,
            _ => InteractionSortBy.UpdatedAt
        };
    }
}
