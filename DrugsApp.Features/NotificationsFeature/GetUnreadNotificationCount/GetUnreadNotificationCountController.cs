using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.NotificationsFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.NotificationsFeature.GetUnreadNotificationCount;

public sealed record GetUnreadNotificationCountQuery(
    Guid UserId,
    bool IncludeProviderAudience) : IRequest<ErrorOr<UnreadNotificationCountResponse>>;

[ApiController]
[Authorize(Roles = $"{AuthRoles.Client},{AuthRoles.HealthcareProvider}")]
[Tags("Notifications")]
[Route("api/notifications")]
public sealed class GetUnreadNotificationCountController(IMediator _mediator) : ControllerBase
{
    public const string RouteName = "GetUnreadNotificationCount";

    [HttpGet("unread-count", Name = RouteName)]
    [ProducesResponseType(typeof(UnreadNotificationCountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UnreadNotificationCountResponse>> GetUnreadCount(
        CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        bool includeProviderAudience = User.IsInRole(AuthRoles.HealthcareProvider);
        GetUnreadNotificationCountQuery query = new(userId, includeProviderAudience);
        ErrorOr<UnreadNotificationCountResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<UnreadNotificationCountResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
