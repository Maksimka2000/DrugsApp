using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.UserProfileFeature.RemoveHealthcareProviderProfile;

[ApiController]
[Route("api/healthcare-provider/profile")]
[Authorize(Roles = "HealthcareProvider")]
[Tags("User Profile")]
public sealed class RemoveHealthcareProviderProfileController(IMediator _mediator) : ControllerBase
{
    public const string RouteName = "RemoveHealthcareProviderProfile";

    /// <summary>
    /// Removes the healthcare provider profile and role for the current provider user.
    /// </summary>
    [HttpDelete(Name = RouteName)]
    [ProducesResponseType(typeof(RemoveHealthcareProviderProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RemoveHealthcareProviderProfileResponse>> Remove(CancellationToken cancellationToken)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        RemoveHealthcareProviderProfileCommand command = new(userId);
        ErrorOr<RemoveHealthcareProviderProfileResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<RemoveHealthcareProviderProfileResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
