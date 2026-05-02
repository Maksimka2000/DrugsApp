namespace DrugsApp.Features.FoodFeature.CheckFoodGroupNameAvailability;

/// <summary>
/// Mapping helpers for food group name availability flow.
/// </summary>
public static class CheckFoodGroupNameAvailabilityMapping
{
    /// <summary>
    /// Maps request to query with trimmed name.
    /// </summary>
    public static CheckFoodGroupNameAvailabilityQuery MapToQuery(this CheckFoodGroupNameAvailabilityRequest request) =>
        new(
            Name: request.Name.Trim(),
            ExcludeId: request.ExcludeId);

    /// <summary>
    /// Maps availability data to response DTO.
    /// </summary>
    public static CheckFoodGroupNameAvailabilityResponse MapToResponse(
        bool isAvailable,
        Guid? conflictingResourceId) => new(
            IsAvailable: isAvailable,
            ConflictingResourceId: conflictingResourceId);
    
}
