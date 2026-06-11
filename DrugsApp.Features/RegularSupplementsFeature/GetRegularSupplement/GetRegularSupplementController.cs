using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularSupplementsFeature.GetRegularSupplement;

/// <summary>
/// Query for retrieving a single regular supplement schedule.
/// </summary>
public sealed record GetRegularSupplementQuery(Guid UserId, Guid Id)
    : IRequest<ErrorOr<GetRegularSupplementResponse>>;

/// <summary>
/// Response containing a regular supplement schedule projection.
/// </summary>
public sealed record GetRegularSupplementResponse(RegularSupplementDetailsDto Item);

/// <summary>
/// Controller for retrieving a regular supplement schedule.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Supplements")]
[Route("api/regular-supplements")]
public sealed class GetRegularSupplementController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for retrieving a regular supplement schedule.
    /// </summary>
    public const string RouteName = "GetRegularSupplement";

    /// <summary>
    /// Retrieves a regular supplement schedule owned by the authenticated user.
    /// </summary>
    [HttpGet("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(GetRegularSupplementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GetRegularSupplementResponse>> Get(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return ValidationProblem("id must not be empty.");
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        GetRegularSupplementQuery query = new(userId, id);
        ErrorOr<GetRegularSupplementResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<GetRegularSupplementResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
