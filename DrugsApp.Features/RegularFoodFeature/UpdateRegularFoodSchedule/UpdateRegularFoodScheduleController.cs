using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularFoodFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularFoodFeature.UpdateRegularFoodSchedule;

/// <summary>
/// Request model for updating a regular food schedule.
/// </summary>
public sealed record UpdateRegularFoodScheduleRequest(
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);

/// <summary>
/// Command for updating a regular food schedule.
/// </summary>
public sealed record UpdateRegularFoodScheduleCommand(
    Guid UserId,
    Guid Id,
    UpdateRegularFoodScheduleRequest Request) : IRequest<ErrorOr<UpdateRegularFoodScheduleResponse>>;

/// <summary>
/// Response containing the updated regular food schedule.
/// </summary>
public sealed record UpdateRegularFoodScheduleResponse(RegularFoodListItemDto Item);

/// <summary>
/// Controller for updating regular food schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Regular Food")]
[Route("api/regular-food")]
public sealed class UpdateRegularFoodScheduleController(
    IMediator _mediator,
    IValidator<UpdateRegularFoodScheduleRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for updating a regular food schedule.
    /// </summary>
    public const string RouteName = "UpdateRegularFoodSchedule";

    /// <summary>
    /// Updates schedule dates and intake times for a live regular food row.
    /// </summary>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdateRegularFoodScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdateRegularFoodScheduleResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateRegularFoodScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return ValidationProblem("id must not be empty.");
        }

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        UpdateRegularFoodScheduleCommand command = request.MapToCommand(userId, id);
        ErrorOr<UpdateRegularFoodScheduleResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<UpdateRegularFoodScheduleResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
