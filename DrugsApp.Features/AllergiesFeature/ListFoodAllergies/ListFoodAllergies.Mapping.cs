using System;
using System.Collections.Generic;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.ListFoodAllergies;

/// <summary>
/// Mapping helpers for listing food allergies.
/// </summary>
public static class ListFoodAllergiesMapping
{
    /// <summary>
    /// Maps a list request to its query envelope.
    /// </summary>
    /// <param name="request">List food allergies request.</param>
    /// <param name="userId">Identifier of the owning user.</param>
    /// <returns>Query record for MediatR handling.</returns>
    public static ListFoodAllergiesQuery MapToQuery(this ListFoodAllergiesRequest request, Guid userId) =>
        new(userId, request.Skip, request.Take);

    /// <summary>
    /// Maps DTOs to a paged list response.
    /// </summary>
    /// <param name="items">Returned allergy DTOs.</param>
    /// <param name="totalCount">Total count matching the query.</param>
    /// <param name="hasMore">Whether more items are available.</param>
    /// <returns>Response wrapper.</returns>
    public static ListFoodAllergiesResponse MapToResponse(
        this IReadOnlyCollection<ClientFoodAllergyDto> items, int totalCount, bool hasMore) =>
        new(items, totalCount, hasMore);
}
