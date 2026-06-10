using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AllergiesFeature.GetAllergySummary;

/// <summary>
/// Query for retrieving the allergy summary for the current user.
/// </summary>
public sealed record GetAllergySummaryQuery(Guid UserId) : IRequest<ErrorOr<AllergySummaryResponse>>;

/// <summary>
/// Controller for retrieving allergy summary answers.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class GetAllergySummaryController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for getting allergy summary.
    /// </summary>
    public const string RouteName = "GetAllergySummary";

    /// <summary>
    /// Returns the allergy summary for the current user.
    /// </summary>
    [HttpGet("summary", Name = RouteName)]
    [ProducesResponseType(typeof(AllergySummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AllergySummaryResponse>> Get(CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        GetAllergySummaryQuery query = new(userId);
        ErrorOr<AllergySummaryResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<AllergySummaryResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
