using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Enums;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthConditionFeature.Shared;
using DrugsApp.Features.HealthConditionFeature.UpdateHealthCondition;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthConditionFeature.AddHealthCondition;

/// <summary>
/// Request model for creating a health condition.
/// </summary>
public sealed record AddHealthConditionRequest(
    string HealthConditionName,
    string? Description);

/// <summary>
/// Command to create a health condition.
/// </summary>
public sealed record AddHealthConditionCommand(
    AddHealthConditionRequest Request) : IRequest<ErrorOr<HealthConditionDto>>;

/// <summary>
/// Controller for creating catalog health conditions.
/// </summary>
[ApiController]
[Authorize(Policy = nameof(Permission.ManageOther))]
[Tags("Health Conditions")]
[Route("api/health-conditions")]
public sealed class AddHealthConditionController(
    IMediator _mediator,
    IValidator<AddHealthConditionRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for creating a health condition.
    /// </summary>
    public const string RouteName = "AddHealthCondition";

    /// <summary>
    /// Creates a new catalog health condition.
    /// </summary>
    /// <param name="request">Payload containing name and optional description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Created health condition.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="409">Name conflict.</response>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(HealthConditionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HealthConditionDto>> Create(
        [FromBody] AddHealthConditionRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddHealthConditionCommand command = request.MapToCommand();
        ErrorOr<HealthConditionDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<HealthConditionDto>>(
            response => CreatedAtRoute(
                routeName: UpdateHealthCondition.UpdateHealthConditionController.RouteName,
                routeValues: new { id = response.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
