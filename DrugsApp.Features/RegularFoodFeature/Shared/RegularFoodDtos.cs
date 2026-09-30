using System;
using System.Collections.Generic;

namespace DrugsApp.Features.RegularFoodFeature.Shared;

/// <summary>
/// Shared projection for regular food list, detail, and mutation responses.
/// </summary>
public sealed record RegularFoodListItemDto(
    Guid Id,
    Guid FoodId,
    string FoodName,
    IReadOnlyCollection<string> Aliases,
    string Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<TimeOnly> IntakeTimes,
    int DailyIntakeCount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// History list item extending the live projection with archive metadata.
/// </summary>
public sealed record RegularFoodHistoryListItemDto(
    Guid Id,
    Guid FoodId,
    string FoodName,
    IReadOnlyCollection<string> Aliases,
    string Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<TimeOnly> IntakeTimes,
    int DailyIntakeCount,
    string HistoryReason,
    DateTime ArchivedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Compact summary returned after manually moving a schedule to history.
/// </summary>
public sealed record MoveRegularFoodToHistorySummaryDto(
    Guid Id,
    string FoodName,
    string Status,
    string HistoryReason,
    DateTime ArchivedAtUtc);
