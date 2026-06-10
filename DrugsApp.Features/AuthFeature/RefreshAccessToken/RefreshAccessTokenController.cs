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

namespace DrugsApp.Features.AuthFeature.RefreshAccessToken;

public sealed record RefreshAccessTokenRequest(string RefreshToken);

public sealed record RefreshAccessTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds);

public sealed record RefreshAccessTokenCommand(
    RefreshAccessTokenRequest Request) : IRequest<ErrorOr<RefreshAccessTokenResponse>>;

[ApiController]
[AllowAnonymous]
[Tags("Auth")]
[Route("api/auth")]
public sealed class RefreshAccessTokenController(
    IMediator mediator,
    IValidator<RefreshAccessTokenRequest> validator) : ControllerBase
{
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshAccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshAccessTokenResponse>> Refresh(
        [FromBody] RefreshAccessTokenRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        RefreshAccessTokenCommand command = request.MapToCommand();
        ErrorOr<RefreshAccessTokenResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
