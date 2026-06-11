using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularDrugsFeature.GetRegularDrug;

/// <summary>
/// Query for retrieving a single regular drug schedule.
/// </summary>
public sealed record GetRegularDrugQuery(Guid UserId, Guid Id) : IRequest<ErrorOr<GetRegularDrugResponse>>;

/// <summary>
/// Response containing a regular drug schedule projection.
/// </summary>
public sealed record GetRegularDrugResponse(RegularDrugListItemDto Item);

/// <summary>
/// Controller for retrieving a regular drug schedule.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Drugs")]
[Route("api/regular-drugs")]
public sealed class GetRegularDrugController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for retrieving a regular drug schedule.
    /// </summary>
    public const string RouteName = "GetRegularDrug";

    /// <summary>
    /// Retrieves a regular drug schedule owned by the authenticated user.
    /// </summary>
    [HttpGet("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(GetRegularDrugResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GetRegularDrugResponse>> Get(
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

        GetRegularDrugQuery query = new(userId, id);
        ErrorOr<GetRegularDrugResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<GetRegularDrugResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
