using System;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.CheckActiveSubstanceGroupNameAvailability;

/// <summary>
/// Mapping helpers for group name availability.
/// </summary>
public static class CheckActiveSubstanceGroupNameAvailabilityMapping
{
    /// <summary>
    /// Maps request to query.
    /// </summary>
    public static CheckActiveSubstanceGroupNameAvailabilityQuery MapToQuery(
        this CheckActiveSubstanceGroupNameAvailabilityRequest request) => new(
        Name: request.Name,
        ExcludeId: request.ExcludeId);

    /// <summary>
    /// Maps availability state to response DTO.
    /// </summary>
    public static ActiveSubstanceGroupNameAvailabilityResponse MapToResponse(
        bool isAvailable,
        Guid? conflictingGroupId) => new(
        IsAvailable: isAvailable,
        ConflictingGroupId: conflictingGroupId);
}
