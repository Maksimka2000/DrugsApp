using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;

namespace DrugsApp.Features.RouteOfAdministrationFeature.Shared;

/// <summary>
/// Mapping extensions for RouteOfAdministration entity to DTO transformations.
/// </summary>
public static class RouteOfAdministrationMappings
{
    /// <summary>
    /// Maps a RouteOfAdministration entity to its DTO representation.
    /// </summary>
    public static RouteOfAdministrationDto MapToDto(this RouteOfAdministration routeOfAdministration) => new(
        Id: routeOfAdministration.Id,
        RouteOfAdministrationName: routeOfAdministration.RouteOfAdministrationName,
        IsOralIntake: routeOfAdministration.IsOralIntake);

    /// <summary>
    /// Maps a collection of RouteOfAdministration entities to a list of DTOs.
    /// </summary>
    public static List<RouteOfAdministrationDto> MapToDtoList(
        this IEnumerable<RouteOfAdministration> routeOfAdministrations)
        => routeOfAdministrations.Select(routeOfAdministration => routeOfAdministration.MapToDto()).ToList();
}


