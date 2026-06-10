using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthConditionFeature.RemoveUserHealthCondition;

/// <summary>
/// Command for removing a user health condition.
/// </summary>
public sealed record RemoveUserHealthConditionCommand(
    Guid UserId,
    Guid Id) : IRequest<ErrorOr<Success>>;

/// <summary>
/// Controller for removing user health conditions.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("User Health Conditions")]
[Route("api/user-health-conditions")]
public sealed class RemoveUserHealthConditionController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for removing a user health condition.
    /// </summary>
    public const string RouteName = "RemoveUserHealthCondition";

    /// <summary>
    /// Removes a user health condition belonging to the current user.
    /// </summary>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Remove(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        RemoveUserHealthConditionCommand command = new(userId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
