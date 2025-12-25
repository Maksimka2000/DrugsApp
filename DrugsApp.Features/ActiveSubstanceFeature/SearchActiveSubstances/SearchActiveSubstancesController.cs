using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ActiveSubstanceFeature.SearchActiveSubstances;

/// <summary>
/// Request model for search query parameters.
/// </summary>
public sealed record SearchActiveSubstancesRequest(
    string? Name,
    int Skip,
    int Take);

/// <summary>
/// Response containing active substance list and paging metadata.
/// </summary>
public sealed record SearchActiveSubstancesResponse(
    List<ActiveSubstanceDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Query for searching active substances with filtering and pagination.
/// </summary>
public sealed record SearchActiveSubstancesQuery(
    string? Name,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchActiveSubstancesResponse>>;

/// <summary>
/// Controller for searching active substances with filtering and pagination.
/// </summary>
[ApiController]
[Tags("Active Substances")]
[Route("api/active-substances")]
public sealed class SearchActiveSubstancesController(
    IMediator mediator,
    IValidator<SearchActiveSubstancesRequest> validator) : ControllerBase
{
    public const string RouteName = "SearchActiveSubstances";

    private readonly IMediator _mediator = mediator;
    private readonly IValidator<SearchActiveSubstancesRequest> _validator = validator;

    /// <summary>
    /// Searches active substances by name with pagination.
    /// </summary>
    /// <param name="name">Optional partial match filter for active substance name.</param>
    /// <param name="skip">Number of results to skip for pagination, defaults to 0.</param>
    /// <param name="take">Number of results to return, defaults to 10.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns a page of active substances.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchActiveSubstancesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchActiveSubstancesResponse>> Search(
        [FromQuery] string? name,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        SearchActiveSubstancesRequest request = new(name, skip, take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchActiveSubstancesQuery query = request.MapToQuery();
        ErrorOr<SearchActiveSubstancesResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}


