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

namespace DrugsApp.Features.RouteOfAdministrationFeature.UpdateRouteOfAdministration;

/// <summary>
/// Request model for updating route of administration fields.
/// </summary>
public sealed record UpdateRouteOfAdministrationRequest(
    string? RouteOfAdministrationName);

/// <summary>
/// Response containing updated route of administration fields.
/// </summary>
public sealed record UpdatedRouteOfAdministrationResponse(
    Guid Id,
    string RouteOfAdministrationName);

/// <summary>
/// Command for updating a route of administration name.
/// </summary>
public sealed record UpdateRouteOfAdministrationCommand(
    Guid RouteOfAdministrationId,
    UpdateRouteOfAdministrationRequest Request) : IRequest<ErrorOr<UpdatedRouteOfAdministrationResponse>>;

/// <summary>
/// Controller for updating route of administration information.
/// </summary>
[ApiController]
[Tags("Routes Of Administration")]
[Route("api/routes-of-administration")]
public sealed class UpdateRouteOfAdministrationController(
    IMediator mediator,
    IValidator<UpdateRouteOfAdministrationRequest> validator) : ControllerBase
{
    public const string RouteName = "UpdateRouteOfAdministration";

    private readonly IMediator _mediator = mediator;
    private readonly IValidator<UpdateRouteOfAdministrationRequest> _validator = validator;

    /// <summary>
    /// Updates the name of a route of administration.
    /// </summary>
    /// <param name="id">Route of administration unique identifier.</param>
    /// <param name="request">Update request containing new route of administration name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated route of administration information.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">Route of administration not found.</response>
    /// <response code="409">A route of administration with the same name already exists.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedRouteOfAdministrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdatedRouteOfAdministrationResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateRouteOfAdministrationRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateRouteOfAdministrationCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedRouteOfAdministrationResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}


