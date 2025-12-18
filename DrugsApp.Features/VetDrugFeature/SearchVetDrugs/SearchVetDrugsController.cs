using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.VetDrugFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.VetDrugFeature.SearchVetDrugs;


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
[ApiController]
[Route("api/vet-drugs")]
public sealed class SearchVetDrugsController : ControllerBase
{
    public const string RouteName = "SearchVetDrugs";

    private readonly IMediator _mediator;
    private readonly IValidator<SearchVetDrugsRequest> _validator;

    public SearchVetDrugsController(
        IMediator mediator,
        IValidator<SearchVetDrugsRequest> validator)
    {
        _mediator = mediator;
        _validator = validator;
    }

    /// <summary>
    /// Searches veterinary drugs by commercial name with sorting and pagination.
    /// </summary>
    /// <param name="name">Optional partial match filter for commercial name.</param>
    /// <param name="order">Sort direction (ascending or descending), defaults to ascending.</param>
    /// <param name="skip">Number of results to skip for pagination, defaults to 0.</param>
    /// <param name="take">Number of results to return, defaults to 20.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns a list of matching veterinary drugs.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(List<VetDrugDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<VetDrugDto>>> Search(
        [FromQuery] string? name,
        [FromQuery] string? order,
        [FromQuery] int? skip,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        SearchVetDrugsRequest request = new(name, order, skip, take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchVetDrugsQuery query = SearchVetDrugsMapping.MapToQuery(
            name,
            order ?? "ascending",
            skip ?? 0,
            take ?? 20);

        ErrorOr<List<VetDrugDto>> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext)
        );
    }
}



