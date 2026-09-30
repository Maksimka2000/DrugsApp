using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Enums;

namespace DrugsApp.Features.RegularFoodFeature.Shared;

/// <summary>
/// Mapping helpers for regular food aggregates and DTO representations.
/// </summary>
internal static class RegularFoodMappings
{
    public static RegularFoodListItemDto MapToListItemDto(this ClientRegularFood regularFood) => new(
        Id: regularFood.Id,
        FoodId: regularFood.FoodId,
        FoodName: regularFood.Food.Name,
        Aliases: regularFood.Food.Aliases.ToList(),
        Status: MapStatus(regularFood.Status),
        StartDate: regularFood.StartDate,
        EndDate: regularFood.EndDate,
        IntakeTimes: MapIntakeTimes(regularFood),
        DailyIntakeCount: regularFood.IntakeTimes.Count,
        CreatedAtUtc: regularFood.CreatedAtUtc,
        UpdatedAtUtc: regularFood.UpdatedAtUtc);

    public static RegularFoodHistoryListItemDto MapToHistoryItemDto(this ClientRegularFood regularFood) => new(
        Id: regularFood.Id,
        FoodId: regularFood.FoodId,
        FoodName: regularFood.Food.Name,
        Aliases: regularFood.Food.Aliases.ToList(),
        Status: MapStatus(regularFood.Status),
        StartDate: regularFood.StartDate,
        EndDate: regularFood.EndDate,
        IntakeTimes: MapIntakeTimes(regularFood),
        DailyIntakeCount: regularFood.IntakeTimes.Count,
        HistoryReason: MapHistoryReason(regularFood.HistoryReason ?? RegularFoodHistoryReason.ManualRemove),
        ArchivedAtUtc: regularFood.ArchivedAtUtc ?? regularFood.UpdatedAtUtc ?? regularFood.CreatedAtUtc,
        CreatedAtUtc: regularFood.CreatedAtUtc,
        UpdatedAtUtc: regularFood.UpdatedAtUtc);

    public static MoveRegularFoodToHistorySummaryDto MapToMoveToHistorySummaryDto(this ClientRegularFood regularFood) =>
        new(
            Id: regularFood.Id,
            FoodName: regularFood.Food.Name,
            Status: MapStatus(regularFood.Status),
            HistoryReason: MapHistoryReason(regularFood.HistoryReason ?? RegularFoodHistoryReason.ManualRemove),
            ArchivedAtUtc: regularFood.ArchivedAtUtc ?? DateTime.UtcNow);

    public static string BuildNotificationSummary(ClientRegularFood regularFood) =>
        $"{regularFood.Food.Name} regular food schedule has ended.";

    public static string MapStatus(RegularFoodStatus status) =>
        status switch
        {
            RegularFoodStatus.Scheduled => "scheduled",
            RegularFoodStatus.Active => "active",
            RegularFoodStatus.Expired => "expired",
            _ => status.ToString().ToLowerInvariant(),
        };

    public static string MapHistoryReason(RegularFoodHistoryReason reason) =>
        reason switch
        {
            RegularFoodHistoryReason.AutoExpire => "auto_expire",
            RegularFoodHistoryReason.ManualRemove => "manual_remove",
            _ => reason.ToString().ToLowerInvariant(),
        };

    private static IReadOnlyCollection<TimeOnly> MapIntakeTimes(ClientRegularFood regularFood) =>
        regularFood.IntakeTimes
            .Select(intakeTime => intakeTime.IntakeTime)
            .OrderBy(time => time)
            .ToList();
}
