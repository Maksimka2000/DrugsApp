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

namespace DrugsApp.Features.AuthFeature.ResendConfirmationEmail;

public sealed record ResendConfirmationEmailRequest(string Email);

public sealed record ResendConfirmationEmailResponse(
    Guid UserId,
    string Email);

public sealed record ResendConfirmationEmailCommand(
    ResendConfirmationEmailRequest Request) : IRequest<ErrorOr<ResendConfirmationEmailResponse?>>;

[ApiController]
[AllowAnonymous]
[Tags("Auth")]
[Route("api/auth")]
public sealed class ResendConfirmationEmailController(
    IMediator mediator,
    IValidator<ResendConfirmationEmailRequest> validator) : ControllerBase
{
    [HttpPost("resend-confirmation-email")]
    [ProducesResponseType(typeof(ResendConfirmationEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Resend(
        [FromBody] ResendConfirmationEmailRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ResendConfirmationEmailCommand command = request.MapToCommand();
        ErrorOr<ResendConfirmationEmailResponse?> result = await mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            response => response is null ? NoContent() : Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
