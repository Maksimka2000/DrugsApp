using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;

namespace DrugsApp.Features.MealTimeManagementFeature.Shared;

public static class MealTimeMappings
{
    public static MealTimeListItemDto MapToListItemDto(this ClientMealTime entity) =>
        new(
            Id: entity.Id,
            MealName: entity.MealName,
            IntakeTime: entity.IntakeTime,
            CreatedAtUtc: entity.CreatedAtUtc,
            UpdatedAtUtc: entity.UpdatedAtUtc);

    public static MealTimeDto MapToDto(this ClientMealTime entity) =>
        new(
            Id: entity.Id,
            MealName: entity.MealName,
            IntakeTime: entity.IntakeTime,
            CreatedAtUtc: entity.CreatedAtUtc,
            UpdatedAtUtc: entity.UpdatedAtUtc);

    public static IReadOnlyList<MealTimeListItemDto> MapToListItemDtoList(
        this IEnumerable<ClientMealTime> entities) =>
        entities.Select(entity => entity.MapToListItemDto()).ToList();
}
