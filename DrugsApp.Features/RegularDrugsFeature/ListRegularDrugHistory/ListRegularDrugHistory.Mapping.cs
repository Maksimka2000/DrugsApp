using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Features.RegularDrugsFeature.Shared;

namespace DrugsApp.Features.RegularDrugsFeature.ListRegularDrugHistory;

/// <summary>
/// Mapping helpers for list regular drug history use case.
/// </summary>
internal static class ListRegularDrugHistoryMapping
{
    public static ListRegularDrugHistoryQuery MapToQuery(this ListRegularDrugHistoryRequest request, Guid userId) =>
        new(
            userId,
            request.DrugNameQuery,
            request.StartDate,
            request.EndDate,
            request.Skip,
            request.Take);

    public static ListRegularDrugHistoryResponse MapToResponse(
        this IEnumerable<Domain.RegularDrugsFeature.Entities.ClientRegularDrug> items,
        int? totalCount,
        bool hasMore) =>
        new(
            items.Select(item => item.MapToHistoryItemDto()).ToList(),
            totalCount,
            hasMore);
}
