using System.Collections.Generic;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Domain.RouteOfAdministrationFeature.Entities;

/// <summary>
/// Represents a route of administration lookup entry used to classify drugs.
/// </summary>
public sealed class RouteOfAdministration : BaseEntity
{
    

    private RouteOfAdministration() : base()
    {
    }



    // Navigation properties

    private readonly List<DrugForm> _drugForms = new();
    /// <summary>
    /// Drug forms referencing this route of administration.
    /// </summary>
    public IReadOnlyCollection<DrugForm> DrugForms => _drugForms;



    // Properties

    /// <summary>
    /// The name of the route of administration.
    /// </summary>
    public string RouteOfAdministrationName { get; private set; } = null!;

    /// <summary>
    /// Indicates whether the route is considered oral intake.
    /// </summary>
    public bool IsOralIntake { get; private set; } = false;

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

    /// <summary>
    /// Sets the oral intake flag for this route.
    /// </summary>
    /// <param name="isOralIntake">Indicates whether the route allows oral intake.</param>
    public void SetIsOralIntake(bool isOralIntake)
    {
        if (IsOralIntake == isOralIntake)
        {
            return;
        }

        IsOralIntake = isOralIntake;
    }
}


