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

namespace DrugsApp.Features.RegularDrugsFeature.ListRegularDrugHistory;

/// <summary>
/// Request model for listing regular drug history rows.
/// </summary>
public sealed record ListRegularDrugHistoryRequest(
    [FromQuery(Name = "drug_name_query")] string? DrugNameQuery,
    [FromQuery(Name = "start_date")] DateOnly? StartDate,
    [FromQuery(Name = "end_date")] DateOnly? EndDate,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 10);

/// <summary>
/// Query for listing regular drug history scoped to the current user.
/// </summary>
public sealed record ListRegularDrugHistoryQuery(
    Guid UserId,
    string? DrugNameQuery,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int Skip,
    int Take) : IRequest<ErrorOr<ListRegularDrugHistoryResponse>>;

/// <summary>
/// Paged response for regular drug history rows.
/// </summary>
public sealed record ListRegularDrugHistoryResponse(
    IReadOnlyCollection<RegularDrugHistoryItemDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Controller for listing regular drug history rows.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberReadRoles)]
[Tags("Regular Drugs")]
[Route("api/regular-drugs/history")]
public sealed class ListRegularDrugHistoryController(
    IMediator _mediator,
    IValidator<ListRegularDrugHistoryRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for listing regular drug history rows.
    /// </summary>
    public const string RouteName = "ListRegularDrugHistory";

    /// <summary>
    /// Lists archived regular drug schedules for the authenticated user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(ListRegularDrugHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListRegularDrugHistoryResponse>> List(
        [FromQuery] ListRegularDrugHistoryRequest request,
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

        ListRegularDrugHistoryQuery query = request.MapToQuery(userId);
        ErrorOr<ListRegularDrugHistoryResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ListRegularDrugHistoryResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
