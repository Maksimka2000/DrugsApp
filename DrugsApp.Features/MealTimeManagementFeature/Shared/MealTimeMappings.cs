using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;

namespace DrugsApp.Features.MealTimeManagementFeature.Shared;

public static class MealTimeMappings
{
    public const string IntakeTimeFormat = "HH\\:mm";

    public static MealTimeListItemDto MapToListItemDto(this ClientMealTime entity) =>
        new(
            Id: entity.Id,
            MealName: entity.MealName,
            IntakeTime: entity.IntakeTime.ToString(IntakeTimeFormat, CultureInfo.InvariantCulture),
            CreatedAtUtc: entity.CreatedAtUtc,
            UpdatedAtUtc: entity.UpdatedAtUtc);

    public static MealTimeDto MapToDto(this ClientMealTime entity) =>
        new(
            Id: entity.Id,
            MealName: entity.MealName,
            IntakeTime: entity.IntakeTime.ToString(IntakeTimeFormat, CultureInfo.InvariantCulture),
            CreatedAtUtc: entity.CreatedAtUtc,
            UpdatedAtUtc: entity.UpdatedAtUtc);

    public static IReadOnlyList<MealTimeListItemDto> MapToListItemDtoList(
        this IEnumerable<ClientMealTime> entities) =>
        entities.Select(entity => entity.MapToListItemDto()).ToList();

    public static bool TryParseIntakeTime(string value, out TimeOnly intakeTime) =>
        TimeOnly.TryParseExact(
            value,
            IntakeTimeFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out intakeTime);
}
