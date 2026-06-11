using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularFoodFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularFoodFeature.ListRegularFood;

/// <summary>
/// Request model for listing live regular food schedules.
/// </summary>
public sealed record ListRegularFoodRequest(
    [FromQuery(Name = "food_name_query")] string? FoodNameQuery,
    [FromQuery(Name = "start_date")] DateOnly? StartDate,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20);

/// <summary>
/// Query for listing live regular food schedules scoped to the current user.
/// </summary>
public sealed record ListRegularFoodQuery(
    Guid UserId,
    string? FoodNameQuery,
    DateOnly? StartDate,
    int Skip,
    int Take) : IRequest<ErrorOr<ListRegularFoodResponse>>;

/// <summary>
/// Paged response for live regular food schedules.
/// </summary>
public sealed record ListRegularFoodResponse(
    IReadOnlyCollection<RegularFoodListItemDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Controller for listing live regular food schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Food")]
[Route("api/regular-food")]
public sealed class ListRegularFoodController(
    IMediator _mediator,
    IValidator<ListRegularFoodRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for listing live regular food schedules.
    /// </summary>
    public const string RouteName = "ListRegularFood";

    /// <summary>
    /// Lists live regular food schedules for the authenticated user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(ListRegularFoodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListRegularFoodResponse>> List(
        [FromQuery] ListRegularFoodRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        ListRegularFoodQuery query = request.MapToQuery(userId);
        ErrorOr<ListRegularFoodResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ListRegularFoodResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
