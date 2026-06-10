using System;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using ErrorOr;

namespace DrugsApp.Features.RegularSupplementsFeature.CheckOwnSupplementNameAvailability;

/// <summary>
/// Mapping helpers for own supplement name availability use case.
/// </summary>
internal static class CheckOwnSupplementNameAvailabilityMapping
{
    public static CheckOwnSupplementNameAvailabilityQuery MapToQuery(
        this CheckOwnSupplementNameAvailabilityRequest request,
        Guid userId,
        Guid? excludeId)
    {
        ErrorOr<(string Display, string Normalized)> normalizedName = ClientOwnSupplement.NormalizeName(request.Name);
        string normalized = normalizedName.IsError
            ? request.Name.Trim().ToLowerInvariant()
            : normalizedName.Value.Normalized;

        return new CheckOwnSupplementNameAvailabilityQuery(userId, normalized, excludeId);
    }
}
