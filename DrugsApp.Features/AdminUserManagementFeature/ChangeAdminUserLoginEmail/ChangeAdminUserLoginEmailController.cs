using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AdminUserManagementFeature.ChangeAdminUserLoginEmail;

[ApiController]
[Route("api/admin-users")]
[Tags("Admin Users")]
public sealed class ChangeAdminUserLoginEmailController(
    IMediator mediator,
    IValidator<ChangeAdminUserLoginEmailRequest> validator) : ControllerBase
{
    public const string RouteName = "ChangeAdminUserLoginEmail";

    [HttpPut("me/login-email", Name = RouteName)]
    public async Task<ActionResult<AdminUserDetailDto>> ChangeLoginEmail(
        [FromBody] ChangeAdminUserLoginEmailRequest request,
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
        var command = new ChangeAdminUserLoginEmailCommand(currentUserId, request);
        ErrorOr<AdminUserDetailDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<AdminUserDetailDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
