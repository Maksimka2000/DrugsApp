using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Features.FoodFeature.Shared;

namespace DrugsApp.Features.FoodFeature.RemoveFood;

/// <summary>
/// Mapping helpers for RemoveFood use case.
/// </summary>
public static class RemoveFoodMapping
{
    /// <summary>
    /// Maps food aggregate to deleted response.
    /// </summary>
    public static DeletedFoodResponse MapToDeletedResponse(this Food food) =>
        new(food.MapToDto());
}
