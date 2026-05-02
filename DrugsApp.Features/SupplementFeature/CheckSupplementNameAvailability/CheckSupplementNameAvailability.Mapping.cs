namespace DrugsApp.Features.SupplementFeature.CheckSupplementNameAvailability;

/// <summary>
/// Mapping helpers for supplement name availability.
/// </summary>
public static class CheckSupplementNameAvailabilityMapping
{
    public static CheckSupplementNameAvailabilityQuery MapToQuery(this CheckSupplementNameAvailabilityRequest request) => new(
        Name: request.Name,
        ExcludeId: request.ExcludeId);
}
