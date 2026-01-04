using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RouteOfAdministrationFeature.CheckRouteOfAdministrationNameAvailability;

/// <summary>
/// Request model for route of administration name availability query parameters.
/// </summary>
public sealed record CheckRouteOfAdministrationNameAvailabilityRequest(
    string Name,
    Guid? ExcludeId);

/// <summary>
/// Response indicating whether a route of administration name is available.
/// </summary>
public sealed record RouteOfAdministrationNameAvailabilityDto(
    bool IsAvailable,
    Guid? ConflictingResourceId);

/// <summary>
/// Query for checking route of administration name availability.
/// </summary>
public sealed record CheckRouteOfAdministrationNameAvailabilityQuery(
    string Name,
    Guid? ExcludeId) : IRequest<ErrorOr<RouteOfAdministrationNameAvailabilityDto>>;

/// <summary>
/// Controller for checking route of administration name availability.
/// </summary>
[ApiController]
[Tags("Routes Of Administration")]
[Route("api/routes-of-administration")]
public sealed class CheckRouteOfAdministrationNameAvailabilityController(
    IMediator _mediator,
    IValidator<CheckRouteOfAdministrationNameAvailabilityRequest> _validator) : ControllerBase
{
    public const string RouteName = "CheckRouteOfAdministrationNameAvailability";

    /// <summary>
    /// Checks whether the provided route of administration name is available.
    /// </summary>
    /// <param name="name">Name to check for availability.</param>
    /// <param name="excludeId">Optional route of administration id to exclude.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns availability information.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet("name-availability", Name = RouteName)]
    [ProducesResponseType(typeof(RouteOfAdministrationNameAvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RouteOfAdministrationNameAvailabilityDto>> Check(
        [FromQuery] string name,
        [FromQuery(Name = "exclude_id")] Guid? excludeId,
        CancellationToken cancellationToken)
    {
        CheckRouteOfAdministrationNameAvailabilityRequest request = new(name, excludeId);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckRouteOfAdministrationNameAvailabilityQuery query = request.MapToQuery();
        ErrorOr<RouteOfAdministrationNameAvailabilityDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}


