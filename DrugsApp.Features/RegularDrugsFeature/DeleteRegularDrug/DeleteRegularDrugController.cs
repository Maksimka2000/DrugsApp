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

namespace DrugsApp.Features.RegularDrugsFeature.DeleteRegularDrug;

/// <summary>
/// Command for permanently deleting a live regular drug schedule.
/// </summary>
public sealed record DeleteRegularDrugCommand(Guid UserId, Guid Id) : IRequest<ErrorOr<Success>>;

/// <summary>
/// Controller for permanently deleting regular drug schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Regular Drugs")]
[Route("api/regular-drugs")]
public sealed class DeleteRegularDrugController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for deleting a regular drug schedule.
    /// </summary>
    public const string RouteName = "DeleteRegularDrug";

    /// <summary>
    /// Permanently deletes a live regular drug schedule owned by the authenticated user.
    /// </summary>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(
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

        DeleteRegularDrugCommand command = new(userId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
