using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AuthFeature.LogoutMember;

public sealed record LogoutMemberRequest(string RefreshToken);

public sealed record LogoutMemberCommand(
    LogoutMemberRequest Request) : IRequest<ErrorOr<Success>>;

[ApiController]
[Tags("Auth")]
[Route("api/auth")]
public sealed class LogoutMemberController(
    IMediator mediator,
    IValidator<LogoutMemberRequest> validator) : ControllerBase
{
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutMemberRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        LogoutMemberCommand command = request.MapToCommand();
        ErrorOr<Success> result = await mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
