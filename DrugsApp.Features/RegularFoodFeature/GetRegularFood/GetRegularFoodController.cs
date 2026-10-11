using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularFoodFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularFoodFeature.GetRegularFood;

/// <summary>
/// Query for retrieving a regular food schedule.
/// </summary>
public sealed record GetRegularFoodQuery(Guid UserId, Guid Id) : IRequest<ErrorOr<GetRegularFoodResponse>>;

/// <summary>
/// Response containing a regular food schedule.
/// </summary>
public sealed record GetRegularFoodResponse(RegularFoodListItemDto Item);

/// <summary>
/// Controller for retrieving regular food schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberReadRoles)]
[Tags("Regular Food")]
[Route("api/regular-food")]
public sealed class GetRegularFoodController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for retrieving a regular food schedule.
    /// </summary>
    public const string RouteName = "GetRegularFood";

    /// <summary>
    /// Retrieves a live regular food schedule for the authenticated user.
    /// </summary>
    [HttpGet("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(GetRegularFoodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GetRegularFoodResponse>> Get(
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

        GetRegularFoodQuery query = new(userId, id);
        ErrorOr<GetRegularFoodResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<GetRegularFoodResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
