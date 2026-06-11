using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularSupplementsFeature.UpdateRegularSupplement;

/// <summary>
/// Request model for updating a regular supplement schedule.
/// </summary>
public sealed record UpdateRegularSupplementRequest(
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);

/// <summary>
/// Command for updating a regular supplement schedule.
/// </summary>
public sealed record UpdateRegularSupplementCommand(
    Guid UserId,
    Guid Id,
    UpdateRegularSupplementRequest Request) : IRequest<ErrorOr<UpdateRegularSupplementResponse>>;

/// <summary>
/// Response containing the updated regular supplement schedule.
/// </summary>
public sealed record UpdateRegularSupplementResponse(RegularSupplementDetailsDto Item);

/// <summary>
/// Controller for updating regular supplement schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Supplements")]
[Route("api/regular-supplements")]
public sealed class UpdateRegularSupplementController(
    IMediator _mediator,
    IValidator<UpdateRegularSupplementRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for updating a regular supplement schedule.
    /// </summary>
    public const string RouteName = "UpdateRegularSupplement";

    /// <summary>
    /// Updates schedule dates and intake times for a live regular supplement row.
    /// </summary>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdateRegularSupplementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdateRegularSupplementResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateRegularSupplementRequest request,
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

        UpdateRegularSupplementCommand command = request.MapToCommand(userId, id);
        ErrorOr<UpdateRegularSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<UpdateRegularSupplementResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
