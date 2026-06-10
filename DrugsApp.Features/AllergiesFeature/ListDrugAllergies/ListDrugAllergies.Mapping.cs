using System;
using System.Collections.Generic;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.ListDrugAllergies;

/// <summary>
/// Mapping helpers for listing drug allergies.
/// </summary>
public static class ListDrugAllergiesMapping
{
    public static ListDrugAllergiesQuery MapToQuery(
        this ListDrugAllergiesRequest request,
        Guid userId) => new(userId, request.Skip, request.Take);

    public static ListDrugAllergiesResponse MapToResponse(
        this IReadOnlyCollection<ClientDrugAllergyDto> items,
        int totalCount,
        bool hasMore) => new(items, totalCount, hasMore);
}
