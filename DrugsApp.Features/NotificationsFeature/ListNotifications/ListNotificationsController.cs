using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.NotificationsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.NotificationsFeature.ListNotifications;

public sealed record ListNotificationsRequest(int Skip, int Take);
public sealed record ListNotificationsQuery(
    Guid UserId,
    bool IncludeProviderAudience,
    int Skip,
    int Take) : IRequest<ErrorOr<NotificationListResponse>>;

[ApiController]
[Authorize(Roles = $"{AuthRoles.Client},{AuthRoles.HealthcareProvider}")]
[Tags("Notifications")]
[Route("api/notifications")]
public sealed class ListNotificationsController(
    IMediator _mediator,
    IValidator<ListNotificationsRequest> _validator) : ControllerBase
{
    public const string RouteName = "ListNotifications";

    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(NotificationListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<NotificationListResponse>> List(
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        CancellationToken cancellationToken = default)
    {
        ListNotificationsRequest request = new(skip, take);
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        bool includeProviderAudience = User.IsInRole(AuthRoles.HealthcareProvider);
        ListNotificationsQuery query = request.MapToQuery(userId, includeProviderAudience);
        ErrorOr<NotificationListResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<NotificationListResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
