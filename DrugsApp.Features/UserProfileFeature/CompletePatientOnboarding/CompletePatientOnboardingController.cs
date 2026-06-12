using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.UserProfileFeature.CompletePatientOnboarding;

[ApiController]
[Route("api/user-profile")]
[Authorize(Roles = AuthRoles.Client)]
[Tags("User Profile")]
public sealed class CompletePatientOnboardingController(IMediator _mediator) : ControllerBase
{
    public const string RouteName = "CompletePatientOnboarding";

    /// <summary>
    /// Marks patient onboarding as complete for the authenticated user.
    /// </summary>
    [HttpPost("complete-onboarding", Name = RouteName)]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>> Post(CancellationToken cancellationToken)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? email = User.FindFirstValue(ClaimTypes.Email);

        if (!Guid.TryParse(subject, out Guid userId) || string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        CompletePatientOnboardingCommand command = new(userId, email);
        ErrorOr<UserProfileResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
