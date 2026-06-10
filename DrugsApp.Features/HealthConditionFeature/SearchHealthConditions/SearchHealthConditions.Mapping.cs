using System.Collections.Generic;
using System.Linq;
using DrugsApp.Features.HealthConditionFeature.Shared;

namespace DrugsApp.Features.HealthConditionFeature.SearchHealthConditions;

/// <summary>
/// Mapping helpers for search health conditions use case.
/// </summary>
public static class SearchHealthConditionsMapping
{
    public static SearchHealthConditionsQuery MapToQuery(this SearchHealthConditionsRequest request)
    {
        return new SearchHealthConditionsQuery(
            HealthConditionNameQuery: NormalizeQuery(request.HealthConditionNameQuery),
            Skip: request.Skip,
            Take: request.Take);
    }

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
