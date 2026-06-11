using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AuthFeature.LoginAdmin;

public sealed record LoginAdminRequest(
    string Email,
    string Password);

public sealed record LoginAdminResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds);

public sealed record LoginAdminCommand(
    LoginAdminRequest Request) : IRequest<ErrorOr<LoginAdminResponse>>;

[ApiController]
[AllowAnonymous]
[Tags("Auth")]
[Route("api/auth")]
public sealed class LoginAdminController(
    IMediator mediator,
    IValidator<LoginAdminRequest> validator) : ControllerBase
{
    [HttpPost("admin/login")]
    [ProducesResponseType(typeof(LoginAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginAdminResponse>> Login(
        [FromBody] LoginAdminRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        LoginAdminCommand command = request.MapToCommand();
        ErrorOr<LoginAdminResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
