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

namespace DrugsApp.Features.AuthFeature.ForgotPassword;

public sealed record ForgotPasswordRequest(string Email);

public sealed record ForgotPasswordCommand(
    ForgotPasswordRequest Request) : IRequest<ErrorOr<Success>>;

[ApiController]
[AllowAnonymous]
[Tags("Auth")]
[Route("api/auth")]
public sealed class ForgotPasswordController(
    IMediator mediator,
    IValidator<ForgotPasswordRequest> validator) : ControllerBase
{
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ForgotPasswordCommand command = request.MapToCommand();
        ErrorOr<Success> result = await mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
