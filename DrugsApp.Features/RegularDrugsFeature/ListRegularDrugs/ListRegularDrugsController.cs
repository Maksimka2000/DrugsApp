using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularDrugsFeature.ListRegularDrugs;

/// <summary>
/// Request model for listing live regular drug schedules.
/// </summary>
public sealed record ListRegularDrugsRequest(
    [FromQuery(Name = "drug_name_query")] string? DrugNameQuery,
    [FromQuery(Name = "start_date")] DateOnly? StartDate,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 10);

/// <summary>
/// Query for listing live regular drug schedules scoped to the current user.
/// </summary>
public sealed record ListRegularDrugsQuery(
    Guid UserId,
    string? DrugNameQuery,
    DateOnly? StartDate,
    int Skip,
    int Take) : IRequest<ErrorOr<ListRegularDrugsResponse>>;

/// <summary>
/// Paged response for live regular drug schedules.
/// </summary>
public sealed record ListRegularDrugsResponse(
    IReadOnlyCollection<RegularDrugListItemDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Controller for listing live regular drug schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Regular Drugs")]
[Route("api/regular-drugs")]
public sealed class ListRegularDrugsController(
    IMediator _mediator,
    IValidator<ListRegularDrugsRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for listing live regular drug schedules.
    /// </summary>
    public const string RouteName = "ListRegularDrugs";

    /// <summary>
    /// Lists live regular drug schedules for the authenticated user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(ListRegularDrugsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListRegularDrugsResponse>> List(
        [FromQuery] ListRegularDrugsRequest request,
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

        ListRegularDrugsQuery query = request.MapToQuery(userId);
        ErrorOr<ListRegularDrugsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ListRegularDrugsResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
