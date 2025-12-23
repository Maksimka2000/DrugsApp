using DrugsApp.Domain.Common;

namespace DrugsApp.Domain.RouteOfAdministrationFeature.Entities;

/// <summary>
/// Represents a route of administration lookup entry used to classify drugs.
/// </summary>
public sealed class RouteOfAdministration : BaseEntity
{
    private RouteOfAdministration() : base()
    {
    }

    /// <summary>
    /// The name of the route of administration.
    /// </summary>
    public string RouteOfAdministrationName { get; private set; } = null!;

    /// <summary>
    /// Creates a new route of administration aggregate.
    /// </summary>
    /// <param name="routeOfAdministrationName">Route of administration name.</param>
    /// <returns>A new <see cref="RouteOfAdministration"/> instance.</returns>
    public static RouteOfAdministration Create(string routeOfAdministrationName) => new()
    {
        RouteOfAdministrationName = routeOfAdministrationName.Trim()
    };

    /// <summary>
    /// Updates the route of administration name.
    /// </summary>
    /// <param name="routeOfAdministrationName">New route of administration name.</param>
    public void UpdateRouteOfAdministrationName(string routeOfAdministrationName)
    {
        RouteOfAdministrationName = routeOfAdministrationName.Trim();
    }
}


