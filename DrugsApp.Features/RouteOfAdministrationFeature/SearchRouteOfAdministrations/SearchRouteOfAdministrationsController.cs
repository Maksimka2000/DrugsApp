using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RouteOfAdministrationFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RouteOfAdministrationFeature.SearchRouteOfAdministrations;

/// <summary>
/// Request model for search query parameters.
/// </summary>
public sealed record SearchRouteOfAdministrationsRequest(
    string? Name,
    int Skip,
    int Take);

/// <summary>
/// Response containing route of administration list and paging metadata.
/// </summary>
public sealed record SearchRouteOfAdministrationsResponse(
    List<RouteOfAdministrationDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Query for searching routes of administration with filtering and pagination.
/// </summary>
public sealed record SearchRouteOfAdministrationsQuery(
    string? Name,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchRouteOfAdministrationsResponse>>;

/// <summary>
/// Controller for searching routes of administration with filtering and pagination.
/// </summary>
[ApiController]
[Tags("Routes Of Administration")]
[Route("api/routes-of-administration")]
public sealed class SearchRouteOfAdministrationsController(
    IMediator mediator,
    IValidator<SearchRouteOfAdministrationsRequest> validator) : ControllerBase
{
    public const string RouteName = "SearchRouteOfAdministrations";

    private readonly IMediator _mediator = mediator;
    private readonly IValidator<SearchRouteOfAdministrationsRequest> _validator = validator;

    /// <summary>
    /// Searches routes of administration by name with pagination.
    /// </summary>
    /// <param name="name">Optional partial match filter for route of administration name.</param>
    /// <param name="skip">Number of results to skip for pagination, defaults to 0.</param>
    /// <param name="take">Number of results to return, defaults to 10.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns a filtered list of routes of administration.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchRouteOfAdministrationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchRouteOfAdministrationsResponse>> Search(
        [FromQuery] string? name,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        SearchRouteOfAdministrationsRequest request = new(name, skip, take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchRouteOfAdministrationsQuery query = request.MapToQuery();
        ErrorOr<SearchRouteOfAdministrationsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}


