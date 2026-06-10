using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.RegularSupplementsFeature.GetRegularSupplement;

/// <summary>
/// Mapping helpers for get regular supplement use case.
/// </summary>
internal static class GetRegularSupplementMapping
{
    public static GetRegularSupplementResponse MapToResponse(
        this Domain.RegularSupplementsFeature.Entities.ClientRegularSupplement regularSupplement) =>
        new(regularSupplement.MapToDetailsDto());
}
