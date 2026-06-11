using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.Common;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.UserProfileFeature.DownloadUserProfilePhoto;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DrugsApp.Domain.AuthFeature;

namespace DrugsApp.Features.UserProfileFeature.DownloadUserProfilePhoto;

[ApiController]
[Route("api/user-profile/photo")]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("User Profile")]
public sealed class DownloadUserProfilePhotoController(IMediator _mediator) : ControllerBase
{
    public const string RouteName = "DownloadUserProfilePhoto";

    /// <summary>
    /// Downloads the profile photo for the current user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    public async Task<IActionResult> Download(CancellationToken cancellationToken)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        DownloadUserProfilePhotoQuery query = new(userId);
        ErrorOr<FileResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            file => File(file.Stream, file.ContentType),
            errors => errors.MapToProblem(HttpContext));
    }
}
