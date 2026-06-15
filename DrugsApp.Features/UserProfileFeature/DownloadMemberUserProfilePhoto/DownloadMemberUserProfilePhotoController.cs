using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.Common;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.UserProfileFeature.DownloadUserProfilePhoto;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.UserProfileFeature.DownloadMemberUserProfilePhoto;

[ApiController]
[Route("api/user-profiles/{user_id:guid}/photo")]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("User Profile")]
public sealed class DownloadMemberUserProfilePhotoController(IMediator mediator) : ControllerBase
{
    public const string RouteName = "DownloadMemberUserProfilePhoto";

    [HttpGet(Name = RouteName)]
    public async Task<IActionResult> Download(
        [FromRoute(Name = "user_id")] Guid userId,
        CancellationToken cancellationToken)
    {
        DownloadUserProfilePhotoQuery query = new(userId);
        ErrorOr<FileResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            file => File(file.Stream, file.ContentType),
            errors => errors.MapToProblem(HttpContext));
    }
}
