using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Features.FoodFeature.Shared;

namespace DrugsApp.Features.FoodFeature.UpdateFoodGroup;

/// <summary>
/// Mapping helpers for UpdateFoodGroup use case.
/// </summary>
public static class UpdateFoodGroupMapping
{
    /// <summary>
    /// Maps request to command with route id.
    /// </summary>
    public static UpdateFoodGroupCommand MapToCommand(
        this UpdateFoodGroupRequest request,
        Guid id) =>
        new( Id: id, Request: request);

    /// <summary>
    /// Maps food group aggregate to updated response.
    /// </summary>
    public static UpdatedFoodGroupResponse MapToUpdatedResponse(this FoodGroup foodGroup) =>
        new(foodGroup.MapToDto());
}
