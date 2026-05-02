using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Features.FoodFeature.Shared;

namespace DrugsApp.Features.FoodFeature.AddFoodGroup;

/// <summary>
/// Mapping helpers for AddFoodGroup use case.
/// </summary>
public static class AddFoodGroupMapping
{
    /// <summary>
    /// Maps request to command.
    /// </summary>
    public static AddFoodGroupCommand MapToCommand(this AddFoodGroupRequest request) =>
        new(request);

    /// <summary>
    /// Maps food group aggregate to created response.
    /// </summary>
    public static CreatedFoodGroupResponse MapToCreatedResponse(this FoodGroup foodGroup) =>
        new(foodGroup.MapToDto());
}
