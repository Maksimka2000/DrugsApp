using System;
using System.Collections.Generic;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.ListFoodAllergies;

public static class ListFoodAllergiesMapping
{
    public static ListFoodAllergiesQuery MapToQuery(this ListFoodAllergiesRequest request, Guid userId) =>
        new(userId, request.Skip, request.Take);

    public static ListFoodAllergiesResponse MapToResponse(
        this IReadOnlyCollection<ClientFoodAllergyDto> items, int totalCount, bool hasMore) =>
        new(items, totalCount, hasMore);
}
