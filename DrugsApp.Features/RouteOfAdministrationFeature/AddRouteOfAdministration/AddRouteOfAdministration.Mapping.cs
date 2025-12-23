using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;

namespace DrugsApp.Features.RouteOfAdministrationFeature.AddRouteOfAdministration;

/// <summary>
/// Mapping extensions for AddRouteOfAdministration use case.
/// </summary>
public static class AddRouteOfAdministrationMapping
{
    /// <summary>
    /// Maps add request to AddRouteOfAdministrationCommand.
    /// </summary>
    public static AddRouteOfAdministrationCommand MapToCommand(this AddRouteOfAdministrationRequest request) => new(
        Request: request);

    /// <summary>
    /// Maps AddRouteOfAdministrationCommand to RouteOfAdministration entity.
    /// </summary>
    public static RouteOfAdministration MapToEntity(this AddRouteOfAdministrationCommand command) => RouteOfAdministration.Create(
        routeOfAdministrationName: command.Request.RouteOfAdministrationName);

    /// <summary>
    /// Maps RouteOfAdministration entity to CreatedRouteOfAdministrationResponse.
    /// </summary>
    public static CreatedRouteOfAdministrationResponse MapToResponse(this RouteOfAdministration routeOfAdministration) => new(
        Id: routeOfAdministration.Id,
        RouteOfAdministrationName: routeOfAdministration.RouteOfAdministrationName);
}


