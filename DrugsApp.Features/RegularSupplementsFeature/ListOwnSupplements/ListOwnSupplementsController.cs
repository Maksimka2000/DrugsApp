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

namespace DrugsApp.Features.RegularSupplementsFeature.ListOwnSupplements;

/// <summary>
/// Query for listing own supplements scoped to the current user.
/// </summary>
public sealed record ListOwnSupplementsQuery(Guid UserId) : IRequest<ErrorOr<ListOwnSupplementsResponse>>;

/// <summary>
/// Controller for listing own supplements.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberReadRoles)]
[Tags("Own Supplements")]
[Route("api/own-supplements")]
public sealed class ListOwnSupplementsController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for listing own supplements.
    /// </summary>
    public const string RouteName = "ListOwnSupplements";

    /// <summary>
    /// Lists own supplements for the authenticated user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(ListOwnSupplementsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListOwnSupplementsResponse>> List(
        CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        ListOwnSupplementsQuery query = new(userId);
        ErrorOr<ListOwnSupplementsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ListOwnSupplementsResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
