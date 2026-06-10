using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AuthFeature.ConfirmEmail;
using DrugsApp.Features.UserProfileFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AuthFeature.RegisterMember;

public sealed record RegisterMemberRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender);

public sealed record RegisterMemberResponse(
    Guid UserId,
    string Email);

public sealed record RegisterMemberCommand(
    RegisterMemberRequest Request) : IRequest<ErrorOr<RegisterMemberResponse>>;

[ApiController]
[AllowAnonymous]
[Tags("Auth")]
[Route("api/auth")]
public sealed class RegisterMemberController(
    IMediator mediator,
    IValidator<RegisterMemberRequest> validator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterMemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterMemberResponse>> Register(
        [FromBody] RegisterMemberRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        RegisterMemberCommand command = request.MapToCommand();
        ErrorOr<RegisterMemberResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            response => CreatedAtRoute(
                routeName: ConfirmEmailController.RouteName,
                routeValues: new { userId = response.UserId },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
