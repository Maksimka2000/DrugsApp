using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularSupplementsFeature.MoveRegularSupplementToHistory;

/// <summary>
/// Command for moving a live regular supplement schedule to history.
/// </summary>
public sealed record MoveRegularSupplementToHistoryCommand(Guid UserId, Guid Id)
    : IRequest<ErrorOr<MoveRegularSupplementToHistoryResponse>>;

/// <summary>
/// Response containing archive summary metadata.
/// </summary>
public sealed record MoveRegularSupplementToHistoryResponse(MoveRegularSupplementToHistorySummaryDto Item);

/// <summary>
/// Controller for moving regular supplement schedules to history.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Supplements")]
[Route("api/regular-supplements")]
public sealed class MoveRegularSupplementToHistoryController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for moving a regular supplement schedule to history.
    /// </summary>
    public const string RouteName = "MoveRegularSupplementToHistory";

    /// <summary>
    /// Archives a live regular supplement schedule to history for the authenticated user.
    /// </summary>
    [HttpPost("{id:guid}/move-to-history", Name = RouteName)]
    [ProducesResponseType(typeof(MoveRegularSupplementToHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MoveRegularSupplementToHistoryResponse>> MoveToHistory(
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

        MoveRegularSupplementToHistoryCommand command = new(userId, id);
        ErrorOr<MoveRegularSupplementToHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<MoveRegularSupplementToHistoryResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
