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
