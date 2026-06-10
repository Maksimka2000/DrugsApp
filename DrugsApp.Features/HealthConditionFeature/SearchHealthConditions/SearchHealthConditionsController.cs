using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthConditionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthConditionFeature.SearchHealthConditions;

/// <summary>
/// Request model for searching health conditions.
/// </summary>
public sealed record SearchHealthConditionsRequest(
    string? HealthConditionNameQuery,
    int Skip,
    int Take);

/// <summary>
/// Response model for health condition search results.
/// </summary>
public sealed record SearchHealthConditionsResponse(
    List<HealthConditionDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Query contract for searching health conditions.
/// </summary>
public sealed record SearchHealthConditionsQuery(
    string? HealthConditionNameQuery,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchHealthConditionsResponse>>;

/// <summary>
/// Controller for searching and listing health conditions.
/// </summary>
[ApiController]
[Authorize]
[Tags("Health Conditions")]
[Route("api/health-conditions")]
public sealed class SearchHealthConditionsController(
    IMediator _mediator,
    IValidator<SearchHealthConditionsRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for search endpoint.
    /// </summary>
    public const string RouteName = "SearchHealthConditions";

    /// <summary>
    /// Searches catalog health conditions with pagination and optional name filter.
    /// </summary>
    /// <param name="healthConditionNameQuery">Optional case-insensitive name filter.</param>
    /// <param name="skip">Paging offset (>= 0).</param>
    /// <param name="take">Paging size (1..40).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paged list of health conditions.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchHealthConditionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchHealthConditionsResponse>> Search(
        [FromQuery(Name = "health_condition_name_query")] string? healthConditionNameQuery,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10,
        CancellationToken cancellationToken = default)
    {
        SearchHealthConditionsRequest request = new(
            healthConditionNameQuery,
            skip,
            take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchHealthConditionsQuery query = request.MapToQuery();
        ErrorOr<SearchHealthConditionsResponse> result =
            await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
