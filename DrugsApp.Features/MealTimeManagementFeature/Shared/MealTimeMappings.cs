using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;

namespace DrugsApp.Features.MealTimeManagementFeature.Shared;

/// <summary>
/// Mapping helpers for meal time entities.
/// </summary>
public static class MealTimeMappings
{
    /// <summary>
    /// Maps a meal time entity to list item DTO.
    /// </summary>
    /// <param name="entity">Meal time entity.</param>
    /// <returns>List item DTO.</returns>
    public static MealTimeListItemDto MapToListItemDto(this ClientMealTime entity) =>
        new(
            Id: entity.Id,
            MealName: entity.MealName,
            IntakeTime: entity.IntakeTime,
            CreatedAtUtc: entity.CreatedAtUtc,
            UpdatedAtUtc: entity.UpdatedAtUtc);

    /// <summary>
    /// Maps a meal time entity to detail DTO.
    /// </summary>
    /// <param name="entity">Meal time entity.</param>
    /// <returns>Detail DTO.</returns>
    public static MealTimeDto MapToDto(this ClientMealTime entity) =>
        new(
            Id: entity.Id,
            MealName: entity.MealName,
            IntakeTime: entity.IntakeTime,
            CreatedAtUtc: entity.CreatedAtUtc,
            UpdatedAtUtc: entity.UpdatedAtUtc);

    /// <summary>
    /// Maps meal time entities to list item DTO list.
    /// </summary>
    /// <param name="entities">Meal time entities.</param>
    /// <returns>List item DTO collection.</returns>
    public static IReadOnlyList<MealTimeListItemDto> MapToListItemDtoList(
        this IEnumerable<ClientMealTime> entities) =>
        entities.Select(entity => entity.MapToListItemDto()).ToList();
}
