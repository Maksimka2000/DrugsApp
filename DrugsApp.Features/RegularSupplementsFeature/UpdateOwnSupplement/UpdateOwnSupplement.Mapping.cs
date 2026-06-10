using System;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.RegularSupplementsFeature.UpdateOwnSupplement;

/// <summary>
/// Mapping helpers for update own supplement use case.
/// </summary>
internal static class UpdateOwnSupplementMapping
{
    public static UpdateOwnSupplementCommand MapToCommand(
        this UpdateOwnSupplementRequest request,
        Guid userId,
        Guid id) =>
        new(userId, id, request);

    public static UpdateOwnSupplementResponse MapToResponse(this OwnSupplementDetailsDto item) =>
        new(item);
}
