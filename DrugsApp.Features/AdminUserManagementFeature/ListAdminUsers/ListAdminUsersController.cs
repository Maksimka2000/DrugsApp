using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AdminUserManagementFeature.ListAdminUsers;

[ApiController]
[Route("api/admin-users")]
[Tags("Admin Users")]
public sealed class ListAdminUsersController(
    IMediator mediator,
    IValidator<ListAdminUsersRequest> validator) : ControllerBase
{
    public const string RouteName = "ListAdminUsers";

    [HttpGet(Name = RouteName)]
    public async Task<ActionResult<ListAdminUsersResponse>> List(
        [FromQuery] ListAdminUsersRequest request,
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

        ListAdminUsersQuery query = request.MapToQuery(currentUserId);
        ErrorOr<ListAdminUsersResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ListAdminUsersResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
