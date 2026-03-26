using System;

namespace DrugsApp.Features.ActiveSubstanceFeature.CheckActiveSubstanceNameAvailability;

/// <summary>
/// Mapping extensions for CheckActiveSubstanceNameAvailability use case.
/// </summary>
public static class CheckActiveSubstanceNameAvailabilityMapping
{
    /// <summary>
    /// Maps query parameters to CheckActiveSubstanceNameAvailabilityQuery.
    /// </summary>
    public static CheckActiveSubstanceNameAvailabilityQuery MapToQuery(
        this CheckActiveSubstanceNameAvailabilityRequest request) => new(
        Name: request.Name,
        ExcludeId: request.ExcludeId);

    /// <summary>
    /// Maps availability state to ActiveSubstanceNameAvailabilityDto.
    /// </summary>
    public static ActiveSubstanceNameAvailabilityResponse MapToResponse(
        bool isAvailable,
        Guid? conflictingResourceId) => new(
        IsAvailable: isAvailable,
        ConflictingResourceId: conflictingResourceId);
}


