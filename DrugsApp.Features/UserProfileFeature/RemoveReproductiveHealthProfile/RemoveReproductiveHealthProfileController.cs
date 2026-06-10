using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.UserProfileFeature.RemoveReproductiveHealthProfile;

[ApiController]
[Route("api/user-profile/reproductive-health")]
[Authorize(Roles = "Client,HealthcareProvider")]
[Tags("User Profile")]
public sealed class RemoveReproductiveHealthProfileController(IMediator _mediator) : ControllerBase
{
    public const string RouteName = "RemoveReproductiveHealthProfile";

    /// <summary>
    /// Removes the reproductive health profile for the current user.
    /// </summary>
    [HttpDelete(Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(CancellationToken cancellationToken)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        RemoveReproductiveHealthProfileCommand command = new(userId);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
