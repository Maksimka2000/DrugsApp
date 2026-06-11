using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularDrugsFeature.MoveRegularDrugToHistory;

/// <summary>
/// Command for moving a live regular drug schedule to history.
/// </summary>
public sealed record MoveRegularDrugToHistoryCommand(Guid UserId, Guid Id)
    : IRequest<ErrorOr<MoveRegularDrugToHistoryResponse>>;

/// <summary>
/// Response containing archive summary metadata.
/// </summary>
public sealed record MoveRegularDrugToHistoryResponse(MoveRegularDrugToHistorySummaryDto Item);

/// <summary>
/// Controller for moving regular drug schedules to history.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Drugs")]
[Route("api/regular-drugs")]
public sealed class MoveRegularDrugToHistoryController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for moving a regular drug schedule to history.
    /// </summary>
    public const string RouteName = "MoveRegularDrugToHistory";

    /// <summary>
    /// Archives a live regular drug schedule to history for the authenticated user.
    /// </summary>
    [HttpPost("{id:guid}/move-to-history", Name = RouteName)]
    [ProducesResponseType(typeof(MoveRegularDrugToHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MoveRegularDrugToHistoryResponse>> MoveToHistory(
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

        MoveRegularDrugToHistoryCommand command = new(userId, id);
        ErrorOr<MoveRegularDrugToHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<MoveRegularDrugToHistoryResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
