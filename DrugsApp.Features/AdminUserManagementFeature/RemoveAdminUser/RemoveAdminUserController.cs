using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AdminUserManagementFeature.RemoveAdminUser;

[ApiController]
[Route("api/admin-users")]
[Tags("Admin Users")]
public sealed class RemoveAdminUserController(IMediator mediator) : ControllerBase
{
    public const string RouteName = "RemoveAdminUser";

    [HttpDelete("{admin_user_id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(
        [FromRoute(Name = "admin_user_id")] Guid adminUserId,
        CancellationToken cancellationToken)
    {
        if (adminUserId == Guid.Empty)
        {
            return ValidationProblem("admin_user_id must not be empty.");
        }

        if (!User.TryGetUserId(out Guid callerUserId))
        {
            return Unauthorized();
        }

        var command = new RemoveAdminUserCommand(adminUserId, callerUserId);
        ErrorOr<Success> result = await mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
