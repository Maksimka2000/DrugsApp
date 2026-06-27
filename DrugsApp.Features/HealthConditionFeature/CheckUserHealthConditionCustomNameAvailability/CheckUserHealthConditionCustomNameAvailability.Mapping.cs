using System;

namespace DrugsApp.Features.HealthConditionFeature.CheckUserHealthConditionCustomNameAvailability;

/// <summary>
/// Mapping helpers for custom name availability use case.
/// </summary>
public static class CheckUserHealthConditionCustomNameAvailabilityMapping
{
    /// <summary>
    /// Builds a query to verify if a user's custom health condition name is available.
    /// </summary>
    /// <param name="request">Request containing the custom name to normalize.</param>
    /// <param name="userId">Identifier of the user owning the custom name.</param>
    /// <returns>Query with user id and normalized custom health condition name.</returns>
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
