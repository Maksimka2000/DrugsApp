using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.FoodFeature.Entities;

namespace DrugsApp.Features.FoodFeature.Shared;

/// <summary>
/// Mapping helpers for FoodFeature shared DTOs.
/// </summary>
public static class FoodMappings
{
    /// <summary>
    /// Maps a food aggregate to its DTO.
    /// </summary>
    public static FoodDto MapToDto(this Food food) => new(
        Id: food.Id,
        Name: food.Name,
        Aliases: food.Aliases.ToList());

    /// <summary>
    /// Maps a collection of foods to DTO list.
    /// </summary>
    public static List<FoodDto> MapToDtoList(this IEnumerable<Food> foods) =>
        foods.Select(food => food.MapToDto()).ToList();

    /// <summary>
    /// Maps a food group aggregate to its DTO.
    /// </summary>
    public static FoodGroupDto MapToDto(this FoodGroup foodGroup) => new(
        Id: foodGroup.Id,
        Name: foodGroup.Name,
        Foods: foodGroup.Foods.MapToDtoList());

    /// <summary>
    /// Maps a collection of food groups to DTO list.
    /// </summary>
    public static List<FoodGroupDto> MapToDtoList(this IEnumerable<FoodGroup> foodGroups) =>
        foodGroups.Select(foodGroup => foodGroup.MapToDto()).ToList();
}
