using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Features.FoodFeature.Shared;

namespace DrugsApp.Features.FoodFeature.RemoveFoodGroup;

/// <summary>
/// Mapping helpers for RemoveFoodGroup use case.
/// </summary>
public static class RemoveFoodGroupMapping
{
    /// <summary>
    /// Maps food group aggregate to deleted response.
    /// </summary>
    public static DeletedFoodGroupResponse MapToDeletedResponse(this FoodGroup foodGroup) =>
        new(foodGroup.MapToDto());
}
