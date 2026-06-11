using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AdminUserManagementFeature.UpdateAdminUserRole;

[ApiController]
[Route("api/admin-users")]
[Tags("Admin Users")]
public sealed class UpdateAdminUserRoleController(
    IMediator mediator,
    IValidator<UpdateAdminUserRoleRequest> validator) : ControllerBase
{
    public const string RouteName = "UpdateAdminUserRole";

    [HttpPut("{admin_user_id:guid}/role", Name = RouteName)]
    public async Task<ActionResult<AdminUserDetailDto>> Update(
        [FromRoute(Name = "admin_user_id")] Guid adminUserId,
        [FromBody] UpdateAdminUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid currentUserId))
        {
            return Unauthorized();
        }

        var command = new UpdateAdminUserRoleCommand(adminUserId, currentUserId, request);
        ErrorOr<AdminUserDetailDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<AdminUserDetailDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
