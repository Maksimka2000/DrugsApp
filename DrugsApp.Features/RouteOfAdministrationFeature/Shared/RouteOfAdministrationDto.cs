using System;

namespace DrugsApp.Features.RouteOfAdministrationFeature.Shared;

/// <summary>
/// Data transfer object representing a route of administration.
/// </summary>
public sealed record RouteOfAdministrationDto(
    Guid Id,
    string RouteOfAdministrationName,
    bool IsOralIntake);


