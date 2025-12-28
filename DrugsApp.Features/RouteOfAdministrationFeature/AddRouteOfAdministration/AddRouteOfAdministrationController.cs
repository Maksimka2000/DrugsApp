using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RouteOfAdministrationFeature.UpdateRouteOfAdministration;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RouteOfAdministrationFeature.AddRouteOfAdministration;

/// <summary>
/// Request model for adding a route of administration.
/// </summary>
public sealed record AddRouteOfAdministrationRequest(
    string RouteOfAdministrationName,
    bool? IsOralIntake);

/// <summary>
/// Response containing created route of administration details.
/// </summary>
public sealed record CreatedRouteOfAdministrationResponse(
    Guid Id,
    string RouteOfAdministrationName,
    bool IsOralIntake);

/// <summary>
/// Command for adding a new route of administration.
/// </summary>
public sealed record AddRouteOfAdministrationCommand(
    AddRouteOfAdministrationRequest Request) : IRequest<ErrorOr<CreatedRouteOfAdministrationResponse>>;

/// <summary>
/// Controller for adding routes of administration.
/// </summary>
[ApiController]
[Tags("Routes Of Administration")]
[Route("api/routes-of-administration")]
public sealed class AddRouteOfAdministrationController(
    IMediator mediator,
    IValidator<AddRouteOfAdministrationRequest> validator) : ControllerBase
{
    public const string RouteName = "AddRouteOfAdministration";

    private readonly IMediator _mediator = mediator;
    private readonly IValidator<AddRouteOfAdministrationRequest> _validator = validator;

    /// <summary>
    /// Creates a new route of administration.
    /// </summary>
    /// <param name="request">Create request containing route of administration name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Returns the created route of administration.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="409">A route of administration with the same name already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreatedRouteOfAdministrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedRouteOfAdministrationResponse>> Create(
        [FromBody] AddRouteOfAdministrationRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddRouteOfAdministrationCommand command = request.MapToCommand();
        ErrorOr<CreatedRouteOfAdministrationResponse> response = await _mediator.Send(command, cancellationToken);

        return response.Match(
            created => CreatedAtRoute(
                routeName: UpdateRouteOfAdministrationController.RouteName,
                routeValues: new { id = created.Id },
                value: created),
            errors => errors.MapToProblem(HttpContext));
    }
}


