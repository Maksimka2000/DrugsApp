using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Features.FoodFeature.Shared;

namespace DrugsApp.Features.FoodFeature.UpdateFood;

/// <summary>
/// Mapping helpers for UpdateFood use case.
/// </summary>
public static class UpdateFoodMapping
{
    /// <summary>
    /// Maps request and route id to command.
    /// </summary>
    public static UpdateFoodCommand MapToCommand(
        this UpdateFoodRequest request,
        Guid id) => new(
            Id: id, 
            Request: request
        );

    /// <summary>
    /// Maps food aggregate to updated response.
    /// </summary>
    public static UpdatedFoodResponse MapToUpdatedResponse(this Food food) => new(
        Food: food.MapToDto()
    );
}
