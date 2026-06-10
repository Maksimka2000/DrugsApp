using System;
using System.Collections.Generic;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.ListOtherAllergies;

public static class ListOtherAllergiesMapping
{
    public static ListOtherAllergiesQuery MapToQuery(this ListOtherAllergiesRequest request, Guid userId) =>
        new(userId, request.Skip, request.Take);

    public static ListOtherAllergiesResponse MapToResponse(
        this IReadOnlyCollection<ClientOtherAllergyDto> items, int totalCount, bool hasMore) =>
        new(items, totalCount, hasMore);
}
