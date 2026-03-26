using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RouteOfAdministrationFeature.UpdateRouteOfAdministration;

/// <summary>
/// Handles updating a route of administration name.
/// </summary>
internal sealed class UpdateRouteOfAdministrationHandler(IRouteOfAdministrationRepository _routeOfAdministrationRepository)
        : IRequestHandler<UpdateRouteOfAdministrationCommand, ErrorOr<UpdatedRouteOfAdministrationResponse>>
{

    /// <summary>
    /// Updates the route of administration name.
    /// </summary>
    /// <param name="command">Update command containing the route of administration ID and new name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated route of administration response.</returns>
    public async Task<ErrorOr<UpdatedRouteOfAdministrationResponse>> Handle(
        UpdateRouteOfAdministrationCommand command,
        CancellationToken cancellationToken)
    {
        RouteOfAdministration? routeOfAdministration = await _routeOfAdministrationRepository.GetByIdAsync(
            command.RouteOfAdministrationId,
            cancellationToken);

        if (routeOfAdministration is null)
        {
            return Error.NotFound(
                description: "The requested route of administration was not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "RouteOfAdministration",
                    ["resourceId"] = command.RouteOfAdministrationId.ToString()
                });
        }

        if (!string.IsNullOrWhiteSpace(command.Request.RouteOfAdministrationName))
        {
            string normalizedName = command.Request.RouteOfAdministrationName.Trim();

            if (normalizedName != routeOfAdministration.RouteOfAdministrationName)
            {
                RouteOfAdministration? duplicate = await _routeOfAdministrationRepository.GetByNameAsync(
                    normalizedName,
                    cancellationToken);

                if (duplicate is not null && duplicate.Id != routeOfAdministration.Id)
                {
                    return Error.Conflict(
                        description: "A route of administration with the same name already exists.",
                        metadata: new Dictionary<string, object>
                        {
                            ["conflictReason"] = "duplicateRouteOfAdministrationName",
                            ["resourceId"] = duplicate.Id.ToString()
                        });
                }

                routeOfAdministration.UpdateRouteOfAdministrationName(normalizedName);
            }
        }

        if (command.Request.IsOralIntake is bool isOralIntake)
        {
            routeOfAdministration.SetIsOralIntake(isOralIntake);
        }

        return routeOfAdministration.MapToUpdatedResponse();
    }
}


