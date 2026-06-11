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

namespace DrugsApp.Features.RegularSupplementsFeature.GetOwnSupplement;

/// <summary>
/// Query for retrieving a single own supplement.
/// </summary>
public sealed record GetOwnSupplementQuery(Guid UserId, Guid Id) : IRequest<ErrorOr<GetOwnSupplementResponse>>;

/// <summary>
/// Response containing an own supplement projection.
/// </summary>
public sealed record GetOwnSupplementResponse(OwnSupplementDetailsDto Item);

/// <summary>
/// Controller for retrieving an own supplement.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Own Supplements")]
[Route("api/own-supplements")]
public sealed class GetOwnSupplementController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for retrieving an own supplement.
    /// </summary>
    public const string RouteName = "GetOwnSupplement";

    /// <summary>
    /// Retrieves an own supplement owned by the authenticated user.
    /// </summary>
    [HttpGet("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(GetOwnSupplementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetOwnSupplementResponse>> Get(
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

        GetOwnSupplementQuery query = new(userId, id);
        ErrorOr<GetOwnSupplementResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<GetOwnSupplementResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
