namespace DrugsApp.Features.SupplementFeature.CheckSupplementNameAvailability;

/// <summary>
/// Mapping helpers for supplement name availability.
/// </summary>
public static class CheckSupplementNameAvailabilityMapping
{
    /// <summary>
    /// Maps a <see cref="CheckSupplementNameAvailabilityRequest"/> object to a <see cref="CheckSupplementNameAvailabilityQuery"/> object.
    /// </summary>
    /// <param name="request">The request object containing supplement name and optional ID to exclude.</param>
    /// <returns>A query object used to check supplement name availability.</returns>
    public static CheckSupplementNameAvailabilityQuery MapToQuery(this CheckSupplementNameAvailabilityRequest request) => new(
        Name: request.Name,
        ExcludeId: request.ExcludeId);
}
