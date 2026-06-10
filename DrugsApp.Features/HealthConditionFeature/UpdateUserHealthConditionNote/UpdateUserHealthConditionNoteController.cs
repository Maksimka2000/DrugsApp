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

namespace DrugsApp.Features.HealthConditionFeature.UpdateUserHealthConditionNote;

/// <summary>
/// Request model for updating a user health condition note.
/// </summary>
public sealed record UpdateUserHealthConditionNoteRequest(string? Note);

/// <summary>
/// Command for updating a user health condition note.
/// </summary>
public sealed record UpdateUserHealthConditionNoteCommand(
    Guid UserId,
    Guid Id,
    UpdateUserHealthConditionNoteRequest Request) : IRequest<ErrorOr<UserHealthConditionDto>>;

/// <summary>
/// Controller for updating user health condition note.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("User Health Conditions")]
[Route("api/user-health-conditions")]
public sealed class UpdateUserHealthConditionNoteController(
    IMediator _mediator,
    IValidator<UpdateUserHealthConditionNoteRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for updating a user health condition note.
    /// </summary>
    public const string RouteName = "UpdateUserHealthConditionNote";

    /// <summary>
    /// Updates a user health condition note for the current user.
    /// </summary>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UserHealthConditionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserHealthConditionDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateUserHealthConditionNoteRequest request,
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

        UpdateUserHealthConditionNoteCommand command = request.MapToCommand(userId, id);
        ErrorOr<UserHealthConditionDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
