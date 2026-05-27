namespace DrugsApp.Features.SupplementFeature.CheckSupplementGroupNameAvailability;

/// <summary>
/// Mapping helpers for supplement group name availability.
/// </summary>
public static class CheckSupplementGroupNameAvailabilityMapping
{
    /// <summary>
    /// Maps a <see cref="CheckSupplementGroupNameAvailabilityRequest"/> to a
    /// <see cref="CheckSupplementGroupNameAvailabilityQuery"/> object.
    /// </summary>
    /// <param name="request">
    /// The request containing the name of the supplement group and an optional ID to exclude.
    /// </param>
    /// <returns>
    /// A query object used to check supplement group name availability.
    /// </returns>
    public static CheckSupplementGroupNameAvailabilityQuery MapToQuery(this CheckSupplementGroupNameAvailabilityRequest request) => new(    
        Name: request.Name,
        ExcludeId: request.ExcludeId);
}
