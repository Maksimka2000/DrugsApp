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

namespace DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplements;

/// <summary>
/// Request model for listing live regular supplement schedules.
/// </summary>
public sealed record ListRegularSupplementsRequest(
    [FromQuery(Name = "name_query")] string? NameQuery,
    [FromQuery(Name = "start_date")] DateOnly? StartDate,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 10);

/// <summary>
/// Query for listing live regular supplement schedules scoped to the current user.
/// </summary>
public sealed record ListRegularSupplementsQuery(
    Guid UserId,
    string? NameQuery,
    DateOnly? StartDate,
    int Skip,
    int Take) : IRequest<ErrorOr<ListRegularSupplementsResponse>>;

/// <summary>
/// Controller for listing live regular supplement schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberReadRoles)]
[Tags("Regular Supplements")]
[Route("api/regular-supplements")]
public sealed class ListRegularSupplementsController(
    IMediator _mediator,
    IValidator<ListRegularSupplementsRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for listing live regular supplement schedules.
    /// </summary>
    public const string RouteName = "ListRegularSupplements";

    /// <summary>
    /// Lists live regular supplement schedules for the authenticated user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(ListRegularSupplementsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListRegularSupplementsResponse>> List(
        [FromQuery] ListRegularSupplementsRequest request,
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

        ListRegularSupplementsQuery query = request.MapToQuery(userId);
        ErrorOr<ListRegularSupplementsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ListRegularSupplementsResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
