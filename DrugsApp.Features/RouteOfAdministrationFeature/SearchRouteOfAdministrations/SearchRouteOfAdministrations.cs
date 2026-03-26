using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using DrugsApp.Features.RouteOfAdministrationFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RouteOfAdministrationFeature.SearchRouteOfAdministrations;

/// <summary>
/// Handles searching routes of administration by name with pagination metadata.
/// </summary>
internal sealed class SearchRouteOfAdministrationsHandler(IRouteOfAdministrationRepository _routeOfAdministrationRepository)
    : IRequestHandler<SearchRouteOfAdministrationsQuery, ErrorOr<SearchRouteOfAdministrationsResponse>>
{

    /// <summary>
    /// Searches routes of administration with paging and optional total count for the first page.
    /// </summary>
    /// <param name="query">Search parameters including name filter and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search response containing items and paging metadata.</returns>
    public async Task<ErrorOr<SearchRouteOfAdministrationsResponse>> Handle(
        SearchRouteOfAdministrationsQuery query,
        CancellationToken cancellationToken)
    {
        // initialize normalized name filter   
        string? normalizedRouteOfAdministrationName = NormalizeRouteOfAdministrationName(query.RouteOfAdministrationName);
        // nomalize filter
        // determine sort order
        // get sort field
        // initialize take plus one
        int takePlusOne = query.Take + 1;
        // initialize total count
        int? totalCount = null;

        // search routes of administration
        List<RouteOfAdministration> routeOfAdministrations = await _routeOfAdministrationRepository.SearchAsync(
            normalizedRouteOfAdministrationName,
            query.Skip,
            takePlusOne,
            cancellationToken);

        // determine if there are more items
        bool hasMore = routeOfAdministrations.Count > query.Take;

        // get page
        List<RouteOfAdministration> page = hasMore
            ? routeOfAdministrations.Take(query.Take).ToList()
            : routeOfAdministrations;

        // map to DTOs
        List<RouteOfAdministrationDto> items = page.MapToDtoList();

        // count total items
        if (query.Skip == 0)
        {
            totalCount = await _routeOfAdministrationRepository.CountAsync(
                normalizedRouteOfAdministrationName,
                cancellationToken);
        }
        
        return items.MapToResponse(totalCount, hasMore);
    }

    private static string? NormalizeRouteOfAdministrationName(string? routeOfAdministrationName)
    {
        if (string.IsNullOrWhiteSpace(routeOfAdministrationName))
        {
            return null;
        }

        return routeOfAdministrationName.Trim();
    }
}


