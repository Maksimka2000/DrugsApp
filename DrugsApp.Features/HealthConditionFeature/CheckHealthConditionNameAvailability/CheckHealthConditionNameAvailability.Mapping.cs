using System;

namespace DrugsApp.Features.HealthConditionFeature.CheckHealthConditionNameAvailability;

/// <summary>
/// Mapping helpers for health condition name availability use case.
/// </summary>
public static class CheckHealthConditionNameAvailabilityMapping
{
    /// <summary>
    /// Creates a query that checks whether a normalized health condition name is available.
    /// </summary>
    /// <param name="request">Incoming request containing the raw name and optional exclusion id.</param>
    /// <returns>Query with normalized name and exclusion id.</returns>
    public static CheckHealthConditionNameAvailabilityQuery MapToQuery(
        this CheckHealthConditionNameAvailabilityRequest request)
    {
        return new CheckHealthConditionNameAvailabilityQuery(
            NormalizedHealthConditionName: NormalizeName(request.HealthConditionName),
            ExcludeId: request.ExcludeId);
    }

    private static string NormalizeName(string value)
    {
        string trimmed = value.Trim();
        return string.Join(
                ' ',
                trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
    }
}
