using System;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.RegularSupplementsFeature.UpdateRegularSupplement;

/// <summary>
/// Mapping helpers for update regular supplement use case.
/// </summary>
internal static class UpdateRegularSupplementMapping
{
    public static UpdateRegularSupplementCommand MapToCommand(
        this UpdateRegularSupplementRequest request,
        Guid userId,
        Guid id) =>
        new(userId, id, request);

    public static UpdateRegularSupplementResponse MapToResponse(this RegularSupplementDetailsDto item) =>
        new(item);
}
