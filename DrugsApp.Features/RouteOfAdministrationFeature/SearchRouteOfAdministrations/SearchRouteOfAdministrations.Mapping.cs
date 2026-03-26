using DrugsApp.Features.RouteOfAdministrationFeature.Shared;

namespace DrugsApp.Features.RouteOfAdministrationFeature.SearchRouteOfAdministrations;

/// <summary>
/// Mapping extensions for SearchRouteOfAdministrations use case.
/// </summary>
public static class SearchRouteOfAdministrationsMapping
{
    /// <summary>
    /// Maps query parameters to SearchRouteOfAdministrationsQuery.
    /// </summary>
    public static SearchRouteOfAdministrationsQuery MapToQuery(
        this SearchRouteOfAdministrationsRequest request) => new(
        RouteOfAdministrationName: request.RouteOfAdministrationName,
        Skip: request.Skip,
        Take: request.Take);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="items"></param>
    /// <param name="totalCount"></param>
    /// <param name="hasMore"></param>
    /// <returns></returns>
    public static SearchRouteOfAdministrationsResponse MapToResponse(
        this List<RouteOfAdministrationDto> items,
        int? totalCount,
        bool hasMore) => new(
        Items: items,
        TotalCount: totalCount,
        HasMore: hasMore);
}


