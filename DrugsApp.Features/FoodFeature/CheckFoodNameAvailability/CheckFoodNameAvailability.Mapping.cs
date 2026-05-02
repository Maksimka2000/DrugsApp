namespace DrugsApp.Features.FoodFeature.CheckFoodNameAvailability;

/// <summary>
/// Mapping helpers for name availability flow.
/// </summary>
public static class CheckFoodNameAvailabilityMapping
{
    /// <summary>
    /// Maps request to query with trimmed name.
    /// </summary>
    public static CheckFoodNameAvailabilityQuery MapToQuery(this CheckFoodNameAvailabilityRequest request) => new(
            Name: request.Name.Trim(),
            ExcludeId: request.ExcludeId);

    /// <summary>
    /// Maps availability data to response DTO.
    /// </summary>

    public static CheckFoodNameAvailabilityResponse MapToResponse(
        bool isAvailable,
        Guid? conflictingResourceId) => new(
            IsAvailable: isAvailable,
            ConflictingResourceId: conflictingResourceId);

    
}
