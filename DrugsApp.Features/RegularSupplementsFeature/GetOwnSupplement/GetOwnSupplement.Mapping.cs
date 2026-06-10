using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.RegularSupplementsFeature.GetOwnSupplement;

/// <summary>
/// Mapping helpers for get own supplement use case.
/// </summary>
internal static class GetOwnSupplementMapping
{
    public static GetOwnSupplementResponse MapToResponse(
        this Domain.RegularSupplementsFeature.Entities.ClientOwnSupplement ownSupplement) =>
        new(ownSupplement.MapToDetailsDto());
}
