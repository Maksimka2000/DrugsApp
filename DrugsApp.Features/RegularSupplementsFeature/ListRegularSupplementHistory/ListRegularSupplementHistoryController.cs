using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplementHistory;

/// <summary>
/// Request model for listing regular supplement history rows.
/// </summary>
public sealed record ListRegularSupplementHistoryRequest(
    [FromQuery(Name = "name_query")] string? NameQuery,
    [FromQuery(Name = "start_date")] DateOnly? StartDate,
    [FromQuery(Name = "end_date")] DateOnly? EndDate,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 10);

/// <summary>
/// Query for listing regular supplement history scoped to the current user.
/// </summary>
public sealed record ListRegularSupplementHistoryQuery(
    Guid UserId,
    string? NameQuery,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int Skip,
    int Take) : IRequest<ErrorOr<ListRegularSupplementHistoryResponse>>;

/// <summary>
/// Controller for listing regular supplement history rows.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Regular Supplements")]
[Route("api/regular-supplements/history")]
public sealed class ListRegularSupplementHistoryController(
    IMediator _mediator,
    IValidator<ListRegularSupplementHistoryRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for listing regular supplement history rows.
    /// </summary>
    public const string RouteName = "ListRegularSupplementHistory";

    /// <summary>
    /// Lists archived regular supplement schedules for the authenticated user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(ListRegularSupplementHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListRegularSupplementHistoryResponse>> List(
        [FromQuery] ListRegularSupplementHistoryRequest request,
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

        ListRegularSupplementHistoryQuery query = request.MapToQuery(userId);
        ErrorOr<ListRegularSupplementHistoryResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ListRegularSupplementHistoryResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
