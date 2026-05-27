using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.InteractionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.InteractionFeature.SearchInteractions;

/// <summary>
/// Request model for searching interactions by parties.
/// </summary>
public sealed record SearchInteractionsRequest(
    [FromQuery(Name = "interaction_kind")] string InteractionKind,
    [FromQuery(Name = "left_query")] string? LeftQuery,
    [FromQuery(Name = "right_query")] string? RightQuery,
    [FromQuery(Name = "sort_by")] string SortBy = "updated_at",
    [FromQuery(Name = "order_by")] string OrderBy = "descending",
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 10);

/// <summary>
/// Controller for searching interactions.
/// </summary>
[ApiController]
[Tags("ActiveSubstanceInteractions")]
[Route("api/active-substance-interactions")]
public sealed class SearchInteractionsController(
    IMediator mediator,
    IValidator<SearchInteractionsRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "SearchInteractions";

    /// <summary>
    /// Searches interactions by parties and returns paged cards.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchInteractionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchInteractionsResponse>> Search(
        [FromQuery] SearchInteractionsRequest request,
        CancellationToken cancellationToken)
    {
        // run validation
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        // map to query
        SearchInteractionsQuery query = request.MapToQuery();
        ErrorOr<SearchInteractionsResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
