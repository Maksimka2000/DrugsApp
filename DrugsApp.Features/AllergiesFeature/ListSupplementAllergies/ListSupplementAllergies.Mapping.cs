using System;
using System.Collections.Generic;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.ListSupplementAllergies;

public static class ListSupplementAllergiesMapping
{
    public static ListSupplementAllergiesQuery MapToQuery(this ListSupplementAllergiesRequest request, Guid userId) =>
        new(userId, request.Skip, request.Take);

    public static ListSupplementAllergiesResponse MapToResponse(
        this IReadOnlyCollection<ClientSupplementAllergyDto> items, int totalCount, bool hasMore) =>
        new(items, totalCount, hasMore);
}
