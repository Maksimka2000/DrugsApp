using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Features.RegularDrugsFeature.Shared;

namespace DrugsApp.Features.RegularDrugsFeature.ListRegularDrugs;

/// <summary>
/// Mapping helpers for list regular drugs use case.
/// </summary>
internal static class ListRegularDrugsMapping
{
    public static ListRegularDrugsQuery MapToQuery(this ListRegularDrugsRequest request, Guid userId) =>
        new(
            userId,
            request.DrugNameQuery,
            request.StartDate,
            request.Skip,
            request.Take);

    public static ListRegularDrugsResponse MapToResponse(
        this IEnumerable<Domain.RegularDrugsFeature.Entities.ClientRegularDrug> items,
        int? totalCount,
        bool hasMore) =>
        new(
            items.Select(item => item.MapToListItemDto()).ToList(),
            totalCount,
            hasMore);
}
