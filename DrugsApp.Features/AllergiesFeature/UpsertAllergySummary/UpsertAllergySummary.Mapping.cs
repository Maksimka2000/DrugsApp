using System;

namespace DrugsApp.Features.AllergiesFeature.UpsertAllergySummary;

/// <summary>
/// Mapping helpers for upserting allergy summary.
/// </summary>
public static class UpsertAllergySummaryMapping
{
    /// <summary>
    /// Maps an upsert request to its command envelope.
    /// </summary>
    /// <param name="request">Upsert summary payload.</param>
    /// <param name="userId">Identifier of the owning user.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static UpsertAllergySummaryCommand MapToCommand(
        this UpsertAllergySummaryRequest request,
        Guid userId) => new(userId, request);
}
