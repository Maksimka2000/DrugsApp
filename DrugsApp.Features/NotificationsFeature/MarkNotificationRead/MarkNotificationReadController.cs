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

namespace DrugsApp.Features.NotificationsFeature.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(
    Guid UserId,
    Guid NotificationId,
    bool IncludeProviderAudience) : IRequest<ErrorOr<MarkNotificationReadResponse>>;

[ApiController]
[Authorize(Roles = $"{AuthRoles.Client},{AuthRoles.HealthcareProvider}")]
[Tags("Notifications")]
[Route("api/notifications")]
public sealed class MarkNotificationReadController(IMediator _mediator) : ControllerBase
{
    public const string RouteName = "MarkNotificationRead";

    [HttpPost("{id:guid}/mark-read", Name = RouteName)]
    [ProducesResponseType(typeof(MarkNotificationReadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MarkNotificationReadResponse>> MarkRead(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        bool includeProviderAudience = User.IsInRole(AuthRoles.HealthcareProvider);
        MarkNotificationReadCommand command = new(userId, id, includeProviderAudience);
        ErrorOr<MarkNotificationReadResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<MarkNotificationReadResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
