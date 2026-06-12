using System;
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

namespace DrugsApp.Features.AuthFeature.ResetPassword;

public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmNewPassword);

public sealed record ResetPasswordCommand(
    Guid UserId,
    ResetPasswordRequest Request) : IRequest<ErrorOr<Success>>;

[ApiController]
[AllowAnonymous]
[Tags("Auth")]
[Route("api/auth")]
public sealed class ResetPasswordController(
    IMediator mediator,
    IValidator<ResetPasswordRequest> validator) : ControllerBase
{
    public const string RouteName = "ResetPassword";

    [HttpPost("reset-password/{userId:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reset(
        [FromRoute] Guid userId,
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ResetPasswordCommand command = request.MapToCommand(userId);
        ErrorOr<Success> result = await mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
