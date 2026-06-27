using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Features.HealthConditionFeature.Shared;

namespace DrugsApp.Features.HealthConditionFeature.SearchUserHealthConditions;

/// <summary>
/// Mapping helpers for user health condition search.
/// </summary>
public static class SearchUserHealthConditionsMapping
{
    /// <summary>
    /// Creates a query for searching a user's health conditions with normalized display name.
    /// </summary>
    /// <param name="request">Search request including the display name query and paging.</param>
    /// <param name="userId">Identifier of the user whose conditions are queried.</param>
    /// <returns>Query populated with user id, normalized name, and paging.</returns>
    public static SearchUserHealthConditionsQuery MapToQuery(
        this SearchUserHealthConditionsRequest request,
        Guid userId)
    {
        return new SearchUserHealthConditionsQuery(
            UserId: userId,
            DisplayNameQuery: NormalizeQuery(request.DisplayNameQuery),
            Skip: request.Skip,
            Take: request.Take);
    }

    /// <summary>
    /// Projects user health condition search results into a response with pagination metadata.
    /// </summary>
    /// <param name="items">Enumeration of user health condition DTOs.</param>
    /// <param name="totalCount">Optional total item count.</param>
    /// <param name="hasMore">Indicates whether more items are available.</param>
    /// <returns>Response DTO containing items and pagination info.</returns>
    public static SearchUserHealthConditionsResponse MapToResponse(
        this IEnumerable<UserHealthConditionDto> items,
        int? totalCount,
        bool hasMore)
    {
        return new SearchUserHealthConditionsResponse(
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
                trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
    }
}
