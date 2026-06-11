using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AdminUserManagementFeature.CreateAdminUser;

[ApiController]
[Route("api/admin-users")]
[Tags("Admin Users")]
public sealed class CreateAdminUserController(
    IMediator mediator,
    IValidator<CreateAdminUserRequest> validator) : ControllerBase
{
    public const string RouteName = "CreateAdminUser";

    [HttpPost(Name = RouteName)]
    public async Task<ActionResult<CreateAdminUserResponse>> Create(
        [FromBody] CreateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateAdminUserCommand command = request.MapToCommand();
        ErrorOr<CreateAdminUserResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<CreateAdminUserResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
