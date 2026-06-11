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

namespace DrugsApp.Features.RegularDrugsFeature.ClearRegularDrugHistory;

/// <summary>
/// Command for clearing all history rows for the current user.
/// </summary>
public sealed record ClearRegularDrugHistoryCommand(Guid UserId) : IRequest<ErrorOr<ClearRegularDrugHistoryResponse>>;

/// <summary>
/// Response containing the number of deleted history rows.
/// </summary>
public sealed record ClearRegularDrugHistoryResponse(int DeletedCount);

/// <summary>
/// Controller for clearing regular drug history rows.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Drugs")]
[Route("api/regular-drugs/history")]
public sealed class ClearRegularDrugHistoryController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for clearing regular drug history rows.
    /// </summary>
    public const string RouteName = "ClearRegularDrugHistory";

    /// <summary>
    /// Permanently deletes all archived regular drug schedules for the authenticated user.
    /// </summary>
    [HttpDelete(Name = RouteName)]
    [ProducesResponseType(typeof(ClearRegularDrugHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClearRegularDrugHistoryResponse>> Clear(
        CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        ClearRegularDrugHistoryCommand command = new(userId);
        ErrorOr<ClearRegularDrugHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<ClearRegularDrugHistoryResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
