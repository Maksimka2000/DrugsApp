using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthConditionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthConditionFeature.AddUserHealthCondition;

/// <summary>
/// Request model for adding a user health condition.
/// </summary>
public sealed record AddUserHealthConditionRequest(
    Guid? HealthConditionId,
    string? CustomHealthConditionName,
    string? Note);

/// <summary>
/// Command for adding a user health condition.
/// </summary>
public sealed record AddUserHealthConditionCommand(
    Guid UserId,
    AddUserHealthConditionRequest Request) : IRequest<ErrorOr<UserHealthConditionDto>>;

/// <summary>
/// Controller for adding user health conditions.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("User Health Conditions")]
[Route("api/user-health-conditions")]
public sealed class AddUserHealthConditionController(
    IMediator _mediator,
    IValidator<AddUserHealthConditionRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for adding a user health condition.
    /// </summary>
    public const string RouteName = "AddUserHealthCondition";

    /// <summary>
    /// Adds a user health condition using either catalog or custom source.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(UserHealthConditionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserHealthConditionDto>> Add(
        [FromBody] AddUserHealthConditionRequest request,
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

        AddUserHealthConditionCommand command = request.MapToCommand(userId);
        ErrorOr<UserHealthConditionDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<UserHealthConditionDto>>(
            response => CreatedAtRoute(
                routeName: UpdateUserHealthConditionNote.UpdateUserHealthConditionNoteController.RouteName,
                routeValues: new { id = response.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
