using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RouteOfAdministrationFeature.AddRouteOfAdministration;

/// <summary>
/// Handles adding a new route of administration with uniqueness checks.
/// </summary>
internal sealed class AddRouteOfAdministrationHandler (IRouteOfAdministrationRepository _routeOfAdministrationRepository)
    : IRequestHandler<AddRouteOfAdministrationCommand, ErrorOr<CreatedRouteOfAdministrationResponse>>
{


    /// <summary>
    /// Creates a new route of administration if no duplicate exists.
    /// </summary>
    /// <param name="command">Create command containing the route of administration name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created route of administration response.</returns>
    public async Task<ErrorOr<CreatedRouteOfAdministrationResponse>> Handle(
        AddRouteOfAdministrationCommand command,
        CancellationToken cancellationToken)
    {
        string normalizedName = command.Request.RouteOfAdministrationName.Trim();

        RouteOfAdministration? existing = await _routeOfAdministrationRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existing is not null)
        {
            return Error.Conflict(
                description: "A route of administration with the same name already exists.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = existing.Id.ToString()
                });
        }

        RouteOfAdministration routeOfAdministration = command.MapToEntity();

        if (command.Request.IsOralIntake is bool isOralIntake)
        {
            routeOfAdministration.SetIsOralIntake(isOralIntake);
        }

        await _routeOfAdministrationRepository.AddAsync(routeOfAdministration, cancellationToken);

        CreatedRouteOfAdministrationResponse response = routeOfAdministration.MapToResponse();

        return response;
    }
}


