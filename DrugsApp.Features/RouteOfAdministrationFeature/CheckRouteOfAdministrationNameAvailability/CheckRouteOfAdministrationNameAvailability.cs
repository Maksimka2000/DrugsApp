using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RouteOfAdministrationFeature.CheckRouteOfAdministrationNameAvailability;

/// <summary>
/// Handles checking route of administration name availability.
/// </summary>
internal sealed class CheckRouteOfAdministrationNameAvailabilityHandler(IRouteOfAdministrationRepository _routeOfAdministrationRepository)
    : IRequestHandler<CheckRouteOfAdministrationNameAvailabilityQuery, ErrorOr<RouteOfAdministrationNameAvailabilityDto>>
{
    /// <summary>
    /// Checks if a route of administration name is available, applying optional exclusion semantics.
    /// </summary>
    /// <param name="query">Availability query containing name and optional exclude id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Availability result describing whether the name is available.</returns>
    public async Task<ErrorOr<RouteOfAdministrationNameAvailabilityDto>> Handle(
        CheckRouteOfAdministrationNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalizedName = query.Name.Trim();

        RouteOfAdministration? existing = await _routeOfAdministrationRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existing is null)
        {
            return CheckRouteOfAdministrationNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingResourceId: null);
        }

        if (query.ExcludeId is not null && query.ExcludeId.Value == existing.Id)
        {
            return CheckRouteOfAdministrationNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingResourceId: null);
        }

        return CheckRouteOfAdministrationNameAvailabilityMapping.MapToResponse(
            isAvailable: false,
            conflictingResourceId: existing.Id);
    }
}


