using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AdminUserManagementFeature.SuspendAdminUser;

[ApiController]
[Route("api/admin-users")]
[Tags("Admin Users")]
public sealed class SuspendAdminUserController(
    IMediator mediator,
    IValidator<SuspendAdminUserRouteRequest> validator) : ControllerBase
{
    public const string RouteName = "SuspendAdminUser";

    [HttpPut("{admin_user_id:guid}/suspension", Name = RouteName)]
    public async Task<ActionResult<AdminUserSuspensionResponse>> Suspend(
        [FromRoute(Name = "admin_user_id")] Guid adminUserId,
        [FromBody] SuspendAdminUserRequest body,
        CancellationToken cancellationToken)
    {
        var request = new SuspendAdminUserRouteRequest(adminUserId, body.IsSuspended);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid callerUserId))
        {
            return Unauthorized();
        }

        var command = new SuspendAdminUserCommand(
            AdminUserId: adminUserId,
            IsSuspended: body.IsSuspended,
            CallerUserId: callerUserId);

        ErrorOr<AdminUserSuspensionResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<AdminUserSuspensionResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
