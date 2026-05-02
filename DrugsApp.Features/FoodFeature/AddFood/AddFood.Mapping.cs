using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Features.FoodFeature.Shared;

namespace DrugsApp.Features.FoodFeature.AddFood;

/// <summary>
/// Mapping helpers for AddFood use case.
/// </summary>
public static class AddFoodMapping
{
    /// <summary>
    /// Maps request to command.
    /// </summary>
    public static AddFoodCommand MapToCommand(this AddFoodRequest request) =>
        new(request);

    /// <summary>
    /// Maps food aggregate to created response.
    /// </summary>
    public static CreatedFoodResponse MapToCreatedResponse(this Food food) =>
        new(food.MapToDto());
}
