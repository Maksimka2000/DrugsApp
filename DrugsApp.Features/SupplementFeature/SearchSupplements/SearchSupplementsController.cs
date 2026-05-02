using System.Collections.Generic;
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

namespace DrugsApp.Features.SupplementFeature.SearchSupplements;

/// <summary>
/// Request model for supplement search query parameters.
/// </summary>
public sealed record SearchSupplementsRequest(
    string? SupplementName,
    int Skip,
    int Take);

/// <summary>
/// Query for searching supplements with paging metadata.
/// </summary>
public sealed record SearchSupplementsQuery(
    string? SupplementName,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchSupplementsResponse>>;

/// <summary>
/// Response envelope for supplement search.
/// </summary>
public sealed record SearchSupplementsResponse(
    IReadOnlyCollection<SupplementDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Controller for searching supplements with pagination.
/// </summary>
[ApiController]
[Tags("Supplements")]
[Route("api/supplements")]
public sealed class SearchSupplementsController(
    IMediator mediator,
    IValidator<SearchSupplementsRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "SearchSupplements";

    /// <summary>
    /// Searches supplements by name or aliases with pagination.
    /// </summary>
    /// <param name="supplementName">Optional partial name or alias filter.</param>
    /// <param name="skip">Number of records to skip, defaults to 0.</param>
    /// <param name="take">Number of records to take, defaults to 20.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns paged supplements.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchSupplementsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchSupplementsResponse>> Search(
        [FromQuery(Name = "supplement_name")] string? supplementName,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10,
        CancellationToken cancellationToken = default)
    {
        SearchSupplementsRequest request = new(
            SupplementName: supplementName,
            Skip: skip,
            Take: take);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchSupplementsQuery query = request.MapToQuery();
        ErrorOr<SearchSupplementsResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
