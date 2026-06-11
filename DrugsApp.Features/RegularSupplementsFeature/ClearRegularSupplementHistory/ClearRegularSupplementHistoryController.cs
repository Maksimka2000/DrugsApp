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

namespace DrugsApp.Features.RegularSupplementsFeature.ClearRegularSupplementHistory;

/// <summary>
/// Command for clearing all history rows for the current user.
/// </summary>
public sealed record ClearRegularSupplementHistoryCommand(Guid UserId)
    : IRequest<ErrorOr<ClearRegularSupplementHistoryResponse>>;

/// <summary>
/// Controller for clearing regular supplement history rows.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Supplements")]
[Route("api/regular-supplements/history")]
public sealed class ClearRegularSupplementHistoryController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for clearing regular supplement history rows.
    /// </summary>
    public const string RouteName = "ClearRegularSupplementHistory";

    /// <summary>
    /// Permanently deletes all archived regular supplement schedules for the authenticated user.
    /// </summary>
    [HttpDelete(Name = RouteName)]
    [ProducesResponseType(typeof(ClearRegularSupplementHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClearRegularSupplementHistoryResponse>> Clear(
        CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        ClearRegularSupplementHistoryCommand command = new(userId);
        ErrorOr<ClearRegularSupplementHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<ClearRegularSupplementHistoryResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
