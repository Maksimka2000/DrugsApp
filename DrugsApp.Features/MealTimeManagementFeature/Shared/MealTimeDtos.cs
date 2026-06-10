using System;
using System.Collections.Generic;

namespace DrugsApp.Features.MealTimeManagementFeature.Shared;

/// <summary>
/// List item representation of a user meal time.
/// </summary>
public sealed record MealTimeListItemDto(
    Guid Id,
    string MealName,
    string IntakeTime,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Full meal time representation returned by create, retrieve, and update endpoints.
/// </summary>
public sealed record MealTimeDto(
    Guid Id,
    string MealName,
    string IntakeTime,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Response wrapper for the list meal times endpoint.
/// </summary>
public sealed record ListMealTimesResponse(IReadOnlyList<MealTimeListItemDto> Items);

/// <summary>
/// Response indicating availability of meal name and intake time for the current user.
/// </summary>
public sealed record MealTimeAvailabilityResponse(
    bool MealNameAvailable,
    bool IntakeTimeAvailable,
    IReadOnlyList<string> ConflictReasons);
