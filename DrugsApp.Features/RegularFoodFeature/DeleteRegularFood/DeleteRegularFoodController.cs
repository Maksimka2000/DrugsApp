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

namespace DrugsApp.Features.RegularFoodFeature.DeleteRegularFood;

/// <summary>
/// Command for permanently deleting a live regular food schedule.
/// </summary>
public sealed record DeleteRegularFoodCommand(Guid UserId, Guid Id) : IRequest<ErrorOr<Success>>;

/// <summary>
/// Controller for deleting regular food schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Regular Food")]
[Route("api/regular-food")]
public sealed class DeleteRegularFoodController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for deleting a regular food schedule.
    /// </summary>
    public const string RouteName = "DeleteRegularFood";

    /// <summary>
    /// Permanently deletes a live regular food schedule for the authenticated user.
    /// </summary>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return ValidationProblem("id must not be empty.");
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        DeleteRegularFoodCommand command = new(userId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
