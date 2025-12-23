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
internal sealed class SearchRouteOfAdministrationsHandler
    : IRequestHandler<SearchRouteOfAdministrationsQuery, ErrorOr<SearchRouteOfAdministrationsResponse>>
{
    private readonly IRouteOfAdministrationRepository _routeOfAdministrationRepository;

    public SearchRouteOfAdministrationsHandler(IRouteOfAdministrationRepository routeOfAdministrationRepository)
    {
        _routeOfAdministrationRepository = routeOfAdministrationRepository;
    }

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
        string? normalizedNameFilter = string.IsNullOrWhiteSpace(query.Name)
            ? null
            : query.Name.Trim();

        List<RouteOfAdministration> pagePlusOne = await _routeOfAdministrationRepository.SearchAsync(
            normalizedNameFilter,
            query.Skip,
            query.Take + 1,
            cancellationToken);

        bool hasMore = pagePlusOne.Count > query.Take;

        List<RouteOfAdministrationDto> items = pagePlusOne
            .Take(query.Take)
            .MapToDtoList();

        int? totalCount = null;
        if (query.Skip == 0)
        {
            totalCount = await _routeOfAdministrationRepository.CountAsync(
                normalizedNameFilter,
                cancellationToken);
        }

        return items.MapToResponse(totalCount, hasMore);
    }
}


