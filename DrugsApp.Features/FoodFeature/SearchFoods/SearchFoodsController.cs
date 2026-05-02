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

namespace DrugsApp.Features.FoodFeature.SearchFoods;

/// <summary>
/// Request model for food search query parameters.
/// </summary>
public sealed record SearchFoodsRequest(
    string? FoodName,
    int Skip,
    int Take);

/// <summary>
/// Query for searching foods with pagination.
/// </summary>
public sealed record SearchFoodsQuery(
    string? SearchText,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchFoodsResponse>>;

/// <summary>
/// Response payload for food search.
/// </summary>
public sealed record SearchFoodsResponse(
    IReadOnlyCollection<FoodDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Controller for searching foods with pagination.
/// </summary>
[ApiController]
[Tags("Foods")]
[Route("api/foods")]
public sealed class SearchFoodsController(
    IMediator mediator,
    IValidator<SearchFoodsRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "SearchFoods";

    /// <summary>
    /// Searches foods by name or aliases with pagination support.
    /// </summary>
    /// <param name="foodName">Optional partial search text across name and aliases.</param>
    /// <param name="skip">Number of records to skip, defaults to 0.</param>
    /// <param name="take">Number of records to take, defaults to 20.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns paged foods.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchFoodsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchFoodsResponse>> Search(
        [FromQuery(Name = "food_name")] string? foodName,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        CancellationToken cancellationToken = default)
    {
        SearchFoodsRequest request = new(FoodName: foodName, Skip: skip, Take: take);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchFoodsQuery query = request.MapToQuery();
        ErrorOr<SearchFoodsResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
