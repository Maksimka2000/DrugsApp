using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AdminUserManagementFeature.ChangeAdminUserPassword;

[ApiController]
[Route("api/admin-users")]
[Tags("Admin Users")]
public sealed class ChangeAdminUserPasswordController(
    IMediator mediator,
    IValidator<ChangeAdminUserPasswordRequest> validator) : ControllerBase
{
    public const string RouteName = "ChangeAdminUserPassword";

    [HttpPut("me/password", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangeAdminUserPasswordRequest request,
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
        var command = new ChangeAdminUserPasswordCommand(currentUserId, request);
        ErrorOr<Success> result = await mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
