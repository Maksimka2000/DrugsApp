using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AdminUserManagementFeature.GetAdminUser;

[ApiController]
[Route("api/admin-users")]
[Tags("Admin Users")]
public sealed class GetAdminUserController(
    IMediator mediator,
    IValidator<GetAdminUserRequest> validator) : ControllerBase
{
    public const string RouteName = "GetAdminUser";

    [HttpGet("{admin_user_id:guid}", Name = RouteName)]
    public async Task<ActionResult<AdminUserDetailDto>> Get(
        [FromRoute(Name = "admin_user_id")] Guid adminUserId,
        CancellationToken cancellationToken)
    {
        var request = new GetAdminUserRequest(adminUserId);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid currentUserId))
        {
            return Unauthorized();
        }

        var query = new GetAdminUserQuery(adminUserId, currentUserId);
        ErrorOr<AdminUserDetailDto> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<AdminUserDetailDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
