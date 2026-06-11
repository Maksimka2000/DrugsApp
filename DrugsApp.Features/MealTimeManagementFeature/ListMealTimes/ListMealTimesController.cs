using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.MealTimeManagementFeature.ListMealTimes;

/// <summary>
/// Query for listing meal times for the current user.
/// </summary>
public sealed record ListMealTimesQuery(Guid UserId) : IRequest<ErrorOr<ListMealTimesResponse>>;

/// <summary>
/// Controller for listing user meal times.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Meal Times")]
[Route("api/meal-times")]
public sealed class ListMealTimesController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for listing meal times.
    /// </summary>
    public const string RouteName = "ListMealTimes";

    /// <summary>
    /// Lists meal times owned by the current user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(ListMealTimesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ListMealTimesResponse>> List(CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        ListMealTimesQuery query = new(userId);
        ErrorOr<ListMealTimesResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ListMealTimesResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
