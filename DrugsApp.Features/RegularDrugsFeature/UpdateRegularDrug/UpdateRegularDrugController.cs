using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularDrugsFeature.UpdateRegularDrug;

/// <summary>
/// Request model for updating a regular drug schedule.
/// </summary>
public sealed record UpdateRegularDrugRequest(
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);

/// <summary>
/// Command for updating a regular drug schedule.
/// </summary>
public sealed record UpdateRegularDrugCommand(
    Guid UserId,
    Guid Id,
    UpdateRegularDrugRequest Request) : IRequest<ErrorOr<UpdateRegularDrugResponse>>;

/// <summary>
/// Response containing the updated regular drug schedule.
/// </summary>
public sealed record UpdateRegularDrugResponse(RegularDrugListItemDto Item);

/// <summary>
/// Controller for updating regular drug schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Drugs")]
[Route("api/regular-drugs")]
public sealed class UpdateRegularDrugController(
    IMediator _mediator,
    IValidator<UpdateRegularDrugRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for updating a regular drug schedule.
    /// </summary>
    public const string RouteName = "UpdateRegularDrug";

    /// <summary>
    /// Updates schedule dates and intake times for a live regular drug row.
    /// </summary>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdateRegularDrugResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdateRegularDrugResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateRegularDrugRequest request,
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

        UpdateRegularDrugCommand command = request.MapToCommand(userId, id);
        ErrorOr<UpdateRegularDrugResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<UpdateRegularDrugResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
