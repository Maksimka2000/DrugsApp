namespace DrugsApp.Features.SupplementFeature.CheckSupplementGroupNameAvailability;

/// <summary>
/// Mapping helpers for supplement group name availability.
/// </summary>
public static class CheckSupplementGroupNameAvailabilityMapping
{
    public static CheckSupplementGroupNameAvailabilityQuery MapToQuery(this CheckSupplementGroupNameAvailabilityRequest request) => new(    
        Name: request.Name,
        ExcludeId: request.ExcludeId);
}
