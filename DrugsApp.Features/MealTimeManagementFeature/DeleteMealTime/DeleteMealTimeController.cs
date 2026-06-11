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

namespace DrugsApp.Features.MealTimeManagementFeature.DeleteMealTime;

/// <summary>
/// Command for deleting a meal time.
/// </summary>
public sealed record DeleteMealTimeCommand(Guid UserId, Guid MealTimeId) : IRequest<ErrorOr<Success>>;

/// <summary>
/// Controller for deleting meal times.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Meal Times")]
[Route("api/meal-times")]
public sealed class DeleteMealTimeController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for deleting a meal time.
    /// </summary>
    public const string RouteName = "DeleteMealTime";

    /// <summary>
    /// Deletes a meal time owned by the current user.
    /// </summary>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        DeleteMealTimeCommand command = new(userId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
