using System;

namespace DrugsApp.Features.RouteOfAdministrationFeature.CheckRouteOfAdministrationNameAvailability;

/// <summary>
/// Mapping extensions for CheckRouteOfAdministrationNameAvailability use case.
/// </summary>
public static class CheckRouteOfAdministrationNameAvailabilityMapping
{
    /// <summary>
    /// Maps query parameters to CheckRouteOfAdministrationNameAvailabilityQuery.
    /// </summary>
    public static CheckRouteOfAdministrationNameAvailabilityQuery MapToQuery(
        this CheckRouteOfAdministrationNameAvailabilityRequest request) => new(
        Name: request.Name,
        ExcludeId: request.ExcludeId);

    /// <summary>
    /// Maps availability state to RouteOfAdministrationNameAvailabilityDto.
    /// </summary>
    public static RouteOfAdministrationNameAvailabilityDto MapToResponse(
        bool isAvailable,
        Guid? conflictingResourceId) => new(
        IsAvailable: isAvailable,
        ConflictingResourceId: conflictingResourceId);
}


