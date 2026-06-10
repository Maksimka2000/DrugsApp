using System;

namespace DrugsApp.Features.AllergiesFeature.UpsertAllergySummary;

/// <summary>
/// Mapping helpers for upserting allergy summary.
/// </summary>
public static class UpsertAllergySummaryMapping
{
    public static UpsertAllergySummaryCommand MapToCommand(
        this UpsertAllergySummaryRequest request,
        Guid userId) => new(userId, request);
}
