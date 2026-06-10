using System;
using DrugsApp.Features.RegularFoodFeature.Shared;

namespace DrugsApp.Features.RegularFoodFeature.UpdateRegularFoodSchedule;

/// <summary>
/// Mapping helpers for update regular food schedule use case.
/// </summary>
internal static class UpdateRegularFoodScheduleMapping
{
    public static UpdateRegularFoodScheduleCommand MapToCommand(
        this UpdateRegularFoodScheduleRequest request,
        Guid userId,
        Guid id) =>
        new(userId, id, request);

    public static UpdateRegularFoodScheduleResponse MapToResponse(this RegularFoodListItemDto item) =>
        new(item);
}
