using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.ActiveSubstanceGroupFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.SearchActiveSubstanceGroups;

/// <summary>
/// Request model for searching active substance groups.
/// </summary>
public sealed record SearchActiveSubstanceGroupsRequest(
    string? ActiveSubstanceName,
    int Skip,
    int Take);

/// <summary>
/// Response containing groups and paging metadata.
/// </summary>
public sealed record SearchActiveSubstanceGroupsResponse(
    List<ActiveSubstanceGroupDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Query for searching active substance groups with optional member-name filter.
/// </summary>
public sealed record SearchActiveSubstanceGroupsQuery(
    string? ActiveSubstanceName,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchActiveSubstanceGroupsResponse>>;

/// <summary>
/// Controller for searching active substance groups.
/// </summary>
[ApiController]
[Tags("Active Substance Groups")]
[Route("api/active-substance-groups")]
public sealed class SearchActiveSubstanceGroupsController(
    IMediator _mediator,
    IValidator<SearchActiveSubstanceGroupsRequest> _validator) : ControllerBase
{
    public const string RouteName = "SearchActiveSubstanceGroups";

    /// <summary>
    /// Searches active substance groups by group or member name.
    /// </summary>
    /// <param name="query_name">Optional filter by group name or member active substance name.</param>
    /// <param name="skip">Number of results to skip.</param>
    /// <param name="take">Number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns a page of active substance groups.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchActiveSubstanceGroupsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchActiveSubstanceGroupsResponse>> Search(
        [FromQuery(Name = "query_name")] string? query_name,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10,
        CancellationToken cancellationToken = default)
    {
        SearchActiveSubstanceGroupsRequest request = new(query_name, skip, take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchActiveSubstanceGroupsQuery query = request.MapToQuery();
        ErrorOr<SearchActiveSubstanceGroupsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
