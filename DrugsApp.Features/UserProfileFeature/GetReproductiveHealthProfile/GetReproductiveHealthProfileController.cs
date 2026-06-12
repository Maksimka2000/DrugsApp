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

namespace DrugsApp.Features.UserProfileFeature.GetReproductiveHealthProfile;

[ApiController]
[Route("api/user-profile/reproductive-health")]
[Authorize(Roles = AuthRoles.Client)]
[Tags("User Profile")]
public sealed class GetReproductiveHealthProfileController(IMediator _mediator) : ControllerBase
{
    public const string RouteName = "GetReproductiveHealthProfile";

    /// <summary>
    /// Returns the reproductive health profile for the authenticated user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(ReproductiveHealthProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReproductiveHealthProfileResponse>> Get(CancellationToken cancellationToken)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        GetReproductiveHealthProfileQuery query = new(userId);
        ErrorOr<ReproductiveHealthProfileResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
