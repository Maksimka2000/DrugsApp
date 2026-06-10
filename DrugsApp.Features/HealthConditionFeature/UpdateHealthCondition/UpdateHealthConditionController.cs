using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Enums;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthConditionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthConditionFeature.UpdateHealthCondition;

/// <summary>
/// Request model for updating a health condition.
/// </summary>
public sealed record UpdateHealthConditionRequest(
    string? HealthConditionName,
    string? Description);

/// <summary>
/// Command for updating a health condition.
/// </summary>
public sealed record UpdateHealthConditionCommand(
    Guid Id,
    UpdateHealthConditionRequest Request) : IRequest<ErrorOr<HealthConditionDto>>;

/// <summary>
/// Controller for updating catalog health conditions.
/// </summary>
[ApiController]
[Authorize(Policy = nameof(Permission.ManageOther))]
[Tags("Health Conditions")]
[Route("api/health-conditions")]
public sealed class UpdateHealthConditionController(
    IMediator _mediator,
    IValidator<UpdateHealthConditionRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for updating a health condition.
    /// </summary>
    public const string RouteName = "UpdateHealthCondition";

    /// <summary>
    /// Updates a catalog health condition.
    /// </summary>
    /// <param name="id">Health condition identifier.</param>
    /// <param name="request">Patch payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Updated health condition.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Health condition not found.</response>
    /// <response code="409">Name conflict.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(HealthConditionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HealthConditionDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateHealthConditionRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateHealthConditionCommand command = request.MapToCommand(id);
        ErrorOr<HealthConditionDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
