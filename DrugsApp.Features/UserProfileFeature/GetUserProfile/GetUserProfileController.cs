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
using DrugsApp.Domain.AuthFeature;

namespace DrugsApp.Features.UserProfileFeature.GetUserProfile;

[ApiController]
[Route("api/user-profile")]
[Authorize(Roles = AuthRoles.MemberReadRoles)]
[Tags("User Profile")]
public sealed class GetUserProfileController(IMediator _mediator) : ControllerBase
{
    public const string RouteName = "GetUserProfile";

    /// <summary>
    /// Retrieves personal data for the current authenticated user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileResponse>> Get(CancellationToken cancellationToken)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? email = User.FindFirstValue(ClaimTypes.Email);

        if (!Guid.TryParse(subject, out Guid userId) || string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        GetUserProfileQuery query = new(userId, email);

        ErrorOr<UserProfileResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
