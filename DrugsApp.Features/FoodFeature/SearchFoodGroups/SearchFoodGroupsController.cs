using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.FoodFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.FoodFeature.SearchFoodGroups;

/// <summary>
/// Request model for food group search.
/// </summary>
public sealed record SearchFoodGroupsRequest(
    string? FoodNameQuery,
    int Skip,
    int Take);

/// <summary>
/// Query for searching food groups with name/member fallback.
/// </summary>
public sealed record SearchFoodGroupsQuery(
    string? FoodNameQuery,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchFoodGroupsResponse>>;

/// <summary>
/// Response payload for food group search.
/// </summary>
public sealed record SearchFoodGroupsResponse(
    IReadOnlyCollection<FoodGroupDto> Items,
    int? TotalCount,
    bool HasMore);


/// <summary>
/// Controller for searching food groups.
/// </summary>
[ApiController]
[Tags("Food Groups")]
[Route("api/food-groups")]
public sealed class SearchFoodGroupsController(
    IMediator mediator,
    IValidator<SearchFoodGroupsRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "SearchFoodGroups";

    /// <summary>
    /// Searches food groups by name and member foods with fallback strategy.
    /// </summary>
    /// <param name="foodNameQuery">Optional text matching group name or member food names.</param>
    /// <param name="skip">Number of records to skip, defaults to 0.</param>
    /// <param name="take">Number of records to take, defaults to 20.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns paged food groups.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchFoodGroupsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchFoodGroupsResponse>> Search(
        [FromQuery(Name = "food_name_query")] string? foodNameQuery,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new SearchFoodGroupsRequest(foodNameQuery, skip, take);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchFoodGroupsQuery query = request.MapToQuery();
        ErrorOr<SearchFoodGroupsResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
