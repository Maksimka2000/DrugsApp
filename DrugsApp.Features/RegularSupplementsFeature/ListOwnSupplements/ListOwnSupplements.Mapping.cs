using System.Collections.Generic;
using System.Linq;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.RegularSupplementsFeature.ListOwnSupplements;

/// <summary>
/// Mapping helpers for list own supplements use case.
/// </summary>
internal static class ListOwnSupplementsMapping
{
    public static ListOwnSupplementsResponse MapToResponse(
        this IEnumerable<Domain.RegularSupplementsFeature.Entities.ClientOwnSupplement> items) =>
        new(items.Select(item => item.MapToListItemDto()).ToList());
}
