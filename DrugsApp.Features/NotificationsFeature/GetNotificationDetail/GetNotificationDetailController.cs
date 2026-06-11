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

namespace DrugsApp.Features.NotificationsFeature.GetNotificationDetail;

public sealed record GetNotificationDetailQuery(
    Guid UserId,
    Guid NotificationId,
    bool IncludeProviderAudience) : IRequest<ErrorOr<NotificationDetailResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Notifications")]
[Route("api/notifications")]
public sealed class GetNotificationDetailController(IMediator _mediator) : ControllerBase
{
    public const string RouteName = "GetNotificationDetail";

    [HttpGet("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(NotificationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationDetailResponse>> GetDetail(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        bool includeProviderAudience = User.IsInRole(AuthRoles.HealthcareProvider);
        GetNotificationDetailQuery query = new(userId, id, includeProviderAudience);
        ErrorOr<NotificationDetailResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<NotificationDetailResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
