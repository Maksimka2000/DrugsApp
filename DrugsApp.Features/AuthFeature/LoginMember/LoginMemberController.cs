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

namespace DrugsApp.Features.AuthFeature.LoginMember;

public sealed record LoginMemberRequest(
    string Email,
    string Password);

public sealed record LoginMemberResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds);

public sealed record LoginMemberCommand(
    LoginMemberRequest Request) : IRequest<ErrorOr<LoginMemberResponse>>;

[ApiController]
[AllowAnonymous]
[Tags("Auth")]
[Route("api/auth")]
public sealed class LoginMemberController(
    IMediator mediator,
    IValidator<LoginMemberRequest> validator) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginMemberResponse>> Login(
        [FromBody] LoginMemberRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        LoginMemberCommand command = request.MapToCommand();
        ErrorOr<LoginMemberResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
