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

namespace DrugsApp.Features.MealTimeManagementFeature.RetrieveMealTime;

/// <summary>
/// Query for retrieving a meal time by id.
/// </summary>
public sealed record RetrieveMealTimeQuery(Guid UserId, Guid MealTimeId) : IRequest<ErrorOr<MealTimeDto>>;

/// <summary>
/// Controller for retrieving meal times.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberReadRoles)]
[Tags("Meal Times")]
[Route("api/meal-times")]
public sealed class RetrieveMealTimeController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for retrieving a meal time.
    /// </summary>
    public const string RouteName = "RetrieveMealTime";

    /// <summary>
    /// Retrieves a meal time owned by the current user.
    /// </summary>
    [HttpGet("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(MealTimeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MealTimeDto>> Retrieve(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        RetrieveMealTimeQuery query = new(userId, id);
        ErrorOr<MealTimeDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<MealTimeDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
