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

namespace DrugsApp.Features.RegularFoodFeature.ClearRegularFoodHistory;

/// <summary>
/// Command for clearing all history rows for the current user.
/// </summary>
public sealed record ClearRegularFoodHistoryCommand(Guid UserId) : IRequest<ErrorOr<ClearRegularFoodHistoryResponse>>;

/// <summary>
/// Response containing the number of deleted history rows.
/// </summary>
public sealed record ClearRegularFoodHistoryResponse(int DeletedCount);

/// <summary>
/// Controller for clearing regular food history rows.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Regular Food")]
[Route("api/regular-food/history")]
public sealed class ClearRegularFoodHistoryController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for clearing regular food history rows.
    /// </summary>
    public const string RouteName = "ClearRegularFoodHistory";

    /// <summary>
    /// Permanently deletes all archived regular food schedules for the authenticated user.
    /// </summary>
    [HttpDelete(Name = RouteName)]
    [ProducesResponseType(typeof(ClearRegularFoodHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClearRegularFoodHistoryResponse>> Clear(
        CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        ClearRegularFoodHistoryCommand command = new(userId);
        ErrorOr<ClearRegularFoodHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<ClearRegularFoodHistoryResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
