using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularSupplementsFeature.GetRegularSupplement;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularSupplementsFeature.CreateRegularSupplement;

/// <summary>
/// Request model for creating a regular supplement schedule.
/// </summary>
public sealed record CreateRegularSupplementRequest(
    string Source,
    Guid? SupplementId,
    Guid? OwnSupplementId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);

/// <summary>
/// Command for creating a regular supplement schedule.
/// </summary>
public sealed record CreateRegularSupplementCommand(
    Guid UserId,
    CreateRegularSupplementRequest Request) : IRequest<ErrorOr<CreateRegularSupplementResponse>>;

/// <summary>
/// Response containing the created regular supplement schedule.
/// </summary>
public sealed record CreateRegularSupplementResponse(RegularSupplementDetailsDto Item);

/// <summary>
/// Controller for creating regular supplement schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Supplements")]
[Route("api/regular-supplements")]
public sealed class CreateRegularSupplementController(
    IMediator _mediator,
    IValidator<CreateRegularSupplementRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for creating a regular supplement schedule.
    /// </summary>
    public const string RouteName = "CreateRegularSupplement";

    /// <summary>
    /// Creates a new regular supplement schedule for the authenticated user.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(CreateRegularSupplementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateRegularSupplementResponse>> Create(
        [FromBody] CreateRegularSupplementRequest request,
        CancellationToken cancellationToken = default)
    {
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

        CreateRegularSupplementCommand command = request.MapToCommand(userId);
        ErrorOr<CreateRegularSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<CreateRegularSupplementResponse>>(
            response => CreatedAtRoute(
                GetRegularSupplementController.RouteName,
                new { id = response.Item.Id },
                response),
            errors => errors.MapToProblem(HttpContext));
    }
}
