using System;
using System.Collections.Generic;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.ListOtherAllergies;

/// <summary>
/// Mapping helpers for listing other allergies.
/// </summary>
public static class ListOtherAllergiesMapping
{
    /// <summary>
    /// Maps a list request to its query envelope.
    /// </summary>
    /// <param name="request">List other allergies request.</param>
    /// <param name="userId">Identifier of the owning user.</param>
    /// <returns>Query record for MediatR handling.</returns>
    public static ListOtherAllergiesQuery MapToQuery(this ListOtherAllergiesRequest request, Guid userId) =>
        new(userId, request.Skip, request.Take);

    /// <summary>
    /// Maps DTOs to a paged list response.
    /// </summary>
    /// <param name="items">Returned allergy DTOs.</param>
    /// <param name="totalCount">Total count matching the query.</param>
    /// <param name="hasMore">Whether more items are available.</param>
    /// <returns>Response wrapper.</returns>
    public static ListOtherAllergiesResponse MapToResponse(
        this IReadOnlyCollection<ClientOtherAllergyDto> items, int totalCount, bool hasMore) =>
        new(items, totalCount, hasMore);
}
