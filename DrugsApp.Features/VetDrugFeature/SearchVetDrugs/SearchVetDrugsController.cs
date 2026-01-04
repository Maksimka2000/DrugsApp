using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.VetDrugFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.VetDrugFeature.SearchVetDrugs;



/// <summary>
/// Request model for search query parameters.
/// </summary>
public sealed record SearchVetDrugsRequest(
    string? Name,
    string Order,
    int Skip,
    int Take);

/// <summary>
/// Query for searching veterinary drugs with filtering and pagination.
/// </summary>
public sealed record SearchVetDrugsQuery(
    string? Name,
    string Order,
    int Skip,
    int Take) : IRequest<ErrorOr<List<VetDrugDto>>>;

/// <summary>
/// Controller for searching veterinary drugs with filtering and pagination.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SearchVetDrugsController"/> class.
/// </remarks>
/// <param name="_mediator">MediatR mediator.</param>
/// <param name="_validator">Validator for the request.</param>
[ApiController]
[Tags("Vet Drugs")]
[Route("api/vet-drugs")]
public sealed class SearchVetDrugsController(
    IMediator _mediator,
    IValidator<SearchVetDrugsRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "SearchVetDrugs";

    /// <summary>
    /// Searches veterinary drugs by commercial name with sorting and pagination.
    /// </summary>
    /// <param name="name">Optional partial match filter for commercial name.</param>
    /// <param name="order">Sort direction (ascending or descending), defaults to ascending.</param>
    /// <param name="skip">Number of results to skip for pagination, defaults to 0.</param>
    /// <param name="take">Number of results to return, defaults to 10.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns a list of matching veterinary drugs.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(List<VetDrugDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<VetDrugDto>>> Search(
        [FromQuery(Name = "commercial_name")] string? name,
        CancellationToken cancellationToken,
        [FromQuery(Name = "order_by")] string order = "ascending",
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10
        )
    {
        SearchVetDrugsRequest request = new(name, order, skip, take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchVetDrugsQuery query = request.MapToQuery();

        ErrorOr<List<VetDrugDto>> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext)
        );
    }
}



