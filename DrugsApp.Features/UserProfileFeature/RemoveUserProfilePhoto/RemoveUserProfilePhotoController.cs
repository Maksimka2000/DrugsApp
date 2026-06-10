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

namespace DrugsApp.Features.UserProfileFeature.RemoveUserProfilePhoto;

[ApiController]
[Route("api/user-profile/photo")]
[Authorize(Roles = "Client,HealthcareProvider")]
[Tags("User Profile")]
public sealed class RemoveUserProfilePhotoController(IMediator _mediator) : ControllerBase
{
    public const string RouteName = "RemoveUserProfilePhoto";

    /// <summary>
    /// Removes the profile photo for the current user.
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

        RemoveUserProfilePhotoCommand command = new(userId);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
