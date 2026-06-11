using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularFoodFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularFoodFeature.MoveRegularFoodToHistory;

/// <summary>
/// Command for moving a live regular food schedule to history.
/// </summary>
public sealed record MoveRegularFoodToHistoryCommand(Guid UserId, Guid Id)
    : IRequest<ErrorOr<MoveRegularFoodToHistoryResponse>>;

/// <summary>
/// Response containing archive summary metadata.
/// </summary>
public sealed record MoveRegularFoodToHistoryResponse(MoveRegularFoodToHistorySummaryDto Item);

/// <summary>
/// Controller for moving regular food schedules to history.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Food")]
[Route("api/regular-food")]
public sealed class MoveRegularFoodToHistoryController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for moving a regular food schedule to history.
    /// </summary>
    public const string RouteName = "MoveRegularFoodToHistory";

    /// <summary>
    /// Archives a live regular food schedule to history for the authenticated user.
    /// </summary>
    [HttpPost("{id:guid}/move-to-history", Name = RouteName)]
    [ProducesResponseType(typeof(MoveRegularFoodToHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MoveRegularFoodToHistoryResponse>> MoveToHistory(
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

        MoveRegularFoodToHistoryCommand command = new(userId, id);
        ErrorOr<MoveRegularFoodToHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<MoveRegularFoodToHistoryResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
