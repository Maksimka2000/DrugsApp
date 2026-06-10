using System;

namespace DrugsApp.Features.HealthConditionFeature.CheckHealthConditionNameAvailability;

/// <summary>
/// Mapping helpers for health condition name availability use case.
/// </summary>
public static class CheckHealthConditionNameAvailabilityMapping
{
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
