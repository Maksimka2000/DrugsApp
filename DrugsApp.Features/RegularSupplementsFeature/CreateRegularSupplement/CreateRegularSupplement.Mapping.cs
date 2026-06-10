using System;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.RegularSupplementsFeature.CreateRegularSupplement;

/// <summary>
/// Mapping helpers for create regular supplement use case.
/// </summary>
internal static class CreateRegularSupplementMapping
{
    public static CreateRegularSupplementCommand MapToCommand(this CreateRegularSupplementRequest request, Guid userId) =>
        new(userId, request);

    public static CreateRegularSupplementResponse MapToResponse(this RegularSupplementDetailsDto item) =>
        new(item);
}
