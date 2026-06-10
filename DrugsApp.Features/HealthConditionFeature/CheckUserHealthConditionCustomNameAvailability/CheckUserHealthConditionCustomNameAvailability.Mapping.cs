using System;

namespace DrugsApp.Features.HealthConditionFeature.CheckUserHealthConditionCustomNameAvailability;

/// <summary>
/// Mapping helpers for custom name availability use case.
/// </summary>
public static class CheckUserHealthConditionCustomNameAvailabilityMapping
{
    public static CheckUserHealthConditionCustomNameAvailabilityQuery MapToQuery(
        this CheckUserHealthConditionCustomNameAvailabilityRequest request,
        Guid userId)
    {
        return new CheckUserHealthConditionCustomNameAvailabilityQuery(
            UserId: userId,
            NormalizedCustomHealthConditionName: NormalizeName(request.CustomHealthConditionName));
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
