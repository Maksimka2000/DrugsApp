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

namespace DrugsApp.Features.AuthFeature.ConfirmEmail;

public sealed record ConfirmEmailRequest(string Token);

public sealed record ConfirmEmailCommand(
    Guid UserId,
    ConfirmEmailRequest Request) : IRequest<ErrorOr<Success>>;

[ApiController]
[AllowAnonymous]
[Tags("Auth")]
[Route("api/auth")]
public sealed class ConfirmEmailController(
    IMediator mediator,
    IValidator<ConfirmEmailRequest> validator) : ControllerBase
{
    public const string RouteName = "ConfirmEmail";

    [HttpPost("confirm-email/{userId:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        [FromRoute] Guid userId,
        [FromBody] ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ConfirmEmailCommand command = request.MapToCommand(userId);
        ErrorOr<Success> result = await mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
