using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.SupplementFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.SupplementFeature.SearchSupplementGroups;

/// <summary>
/// Request model for supplement group search.
/// </summary>
public sealed record SearchSupplementGroupsRequest(
    string? SupplementNameQuery,
    int Skip,
    int Take);

/// <summary>
/// Query for searching supplement groups.
/// </summary>
public sealed record SearchSupplementGroupsQuery(
    string? SupplementNameQuery,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchSupplementGroupsResponse>>;

/// <summary>
/// Response envelope for supplement group search.
/// </summary>
public sealed record SearchSupplementGroupsResponse(
    IReadOnlyCollection<SupplementGroupDto> Items,
    int? TotalCount,
    bool HasMore,
    string? MatchedFieldToken,
    List<string> MatchedFailedFieldTokens);



/// <summary>
/// Controller for searching supplement groups.
/// </summary>
[ApiController]
[Tags("SupplementGroups")]
[Route("api/supplement-groups")]
public sealed class SearchSupplementGroupsController(
    IMediator _mediator,
    IValidator<SearchSupplementGroupsRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for searching supplement groups.
    /// </summary>
    public const string RouteName = "SearchSupplementGroups";

    /// <summary>
    /// Searches supplement groups by group name or member supplement name.
    /// </summary>
    /// <param name="supplementNameQuery">Optional filter against group name or member supplement name.</param>
    /// <param name="skip">Number of records to skip, defaults to 0.</param>
    /// <param name="take">Number of records to take, defaults to 20.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns paged supplement groups.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchSupplementGroupsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchSupplementGroupsResponse>> Search(
        [FromQuery(Name = "supplement_name_query")] string? supplementNameQuery,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10,
        CancellationToken cancellationToken = default)
    {
        SearchSupplementGroupsRequest request = new(
            SupplementNameQuery: supplementNameQuery,
            Skip: skip,
            Take: take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchSupplementGroupsQuery query = request.MapToQuery();
        ErrorOr<SearchSupplementGroupsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
