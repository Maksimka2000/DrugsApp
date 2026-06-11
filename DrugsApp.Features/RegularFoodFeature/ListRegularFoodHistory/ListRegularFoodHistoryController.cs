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

namespace DrugsApp.Features.RegularFoodFeature.ListRegularFoodHistory;

/// <summary>
/// Request model for listing regular food history rows.
/// </summary>
public sealed record ListRegularFoodHistoryRequest(
    [FromQuery(Name = "food_name_query")] string? FoodNameQuery,
    [FromQuery(Name = "start_date")] DateOnly? StartDate,
    [FromQuery(Name = "end_date")] DateOnly? EndDate,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20);

/// <summary>
/// Query for listing regular food history scoped to the current user.
/// </summary>
public sealed record ListRegularFoodHistoryQuery(
    Guid UserId,
    string? FoodNameQuery,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int Skip,
    int Take) : IRequest<ErrorOr<ListRegularFoodHistoryResponse>>;

/// <summary>
/// Paged response for regular food history rows.
/// </summary>
public sealed record ListRegularFoodHistoryResponse(
    IReadOnlyCollection<RegularFoodHistoryListItemDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Controller for listing regular food history rows.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Food")]
[Route("api/regular-food/history")]
public sealed class ListRegularFoodHistoryController(
    IMediator _mediator,
    IValidator<ListRegularFoodHistoryRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for listing regular food history rows.
    /// </summary>
    public const string RouteName = "ListRegularFoodHistory";

    /// <summary>
    /// Lists archived regular food schedules for the authenticated user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(ListRegularFoodHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListRegularFoodHistoryResponse>> List(
        [FromQuery] ListRegularFoodHistoryRequest request,
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

        ListRegularFoodHistoryQuery query = request.MapToQuery(userId);
        ErrorOr<ListRegularFoodHistoryResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ListRegularFoodHistoryResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
