using System;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.RegularSupplementsFeature.CreateOwnSupplement;

/// <summary>
/// Mapping helpers for create own supplement use case.
/// </summary>
internal static class CreateOwnSupplementMapping
{
    public static CreateOwnSupplementCommand MapToCommand(this CreateOwnSupplementRequest request, Guid userId) =>
        new(userId, request);

    public static CreateOwnSupplementResponse MapToResponse(this OwnSupplementDetailsDto item) =>
        new(item);
}
