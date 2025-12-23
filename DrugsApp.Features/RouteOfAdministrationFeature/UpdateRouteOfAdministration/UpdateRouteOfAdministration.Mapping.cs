using System;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;

namespace DrugsApp.Features.RouteOfAdministrationFeature.UpdateRouteOfAdministration;

/// <summary>
/// Mapping extensions for UpdateRouteOfAdministration use case.
/// </summary>
public static class UpdateRouteOfAdministrationMapping
{
    /// <summary>
    /// Maps update request and route parameter to UpdateRouteOfAdministrationCommand.
    /// </summary>
    public static UpdateRouteOfAdministrationCommand MapToCommand(
        this UpdateRouteOfAdministrationRequest request,
        Guid routeOfAdministrationId) => new(
        RouteOfAdministrationId: routeOfAdministrationId,
        Request: request);

    /// <summary>
    /// Maps RouteOfAdministration entity to UpdatedRouteOfAdministrationResponse.
    /// </summary>
    public static UpdatedRouteOfAdministrationResponse MapToUpdatedResponse(this RouteOfAdministration routeOfAdministration) => new(
        Id: routeOfAdministration.Id,
        RouteOfAdministrationName: routeOfAdministration.RouteOfAdministrationName);
}


