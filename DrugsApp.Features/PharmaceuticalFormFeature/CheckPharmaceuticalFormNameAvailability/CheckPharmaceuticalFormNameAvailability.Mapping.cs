using System;

namespace DrugsApp.Features.PharmaceuticalFormFeature.CheckPharmaceuticalFormNameAvailability;

/// <summary>
/// Mapping extensions for CheckPharmaceuticalFormNameAvailability use case.
/// </summary>
public static class CheckPharmaceuticalFormNameAvailabilityMapping
{
    /// <summary>
    /// Maps query parameters to CheckPharmaceuticalFormNameAvailabilityQuery.
    /// </summary>
    public static CheckPharmaceuticalFormNameAvailabilityQuery MapToQuery(
        this CheckPharmaceuticalFormNameAvailabilityRequest request) => new(
        Name: request.Name,
        ExcludeId: request.ExcludeId);

    /// <summary>
    /// Maps availability state to PharmaceuticalFormNameAvailabilityDto.
    /// </summary>
    public static PharmaceuticalFormNameAvailabilityDto MapToResponse(
        bool isAvailable,
        Guid? conflictingResourceId) => new(
        IsAvailable: isAvailable,
        ConflictingResourceId: conflictingResourceId);
}


