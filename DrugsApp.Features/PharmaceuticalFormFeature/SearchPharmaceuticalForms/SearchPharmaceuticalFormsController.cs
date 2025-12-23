using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PharmaceuticalFormFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.PharmaceuticalFormFeature.SearchPharmaceuticalForms;

/// <summary>
/// Request model for search query parameters.
/// </summary>
public sealed record SearchPharmaceuticalFormsRequest(
    string? Name,
    int Skip,
    int Take);

/// <summary>
/// Response containing pharmaceutical form list and paging metadata.
/// </summary>
public sealed record SearchPharmaceuticalFormsResponse(
    List<PharmaceuticalFormDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Query for searching pharmaceutical forms with filtering and pagination.
/// </summary>
public sealed record SearchPharmaceuticalFormsQuery(
    string? Name,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchPharmaceuticalFormsResponse>>;

/// <summary>
/// Controller for searching pharmaceutical forms with filtering and pagination.
/// </summary>
[ApiController]
[Tags("Pharmaceutical Forms")]
[Route("api/pharmaceutical-forms")]
public sealed class SearchPharmaceuticalFormsController(
    IMediator mediator,
    IValidator<SearchPharmaceuticalFormsRequest> validator) : ControllerBase
{
    public const string RouteName = "SearchPharmaceuticalForms";

    private readonly IMediator _mediator = mediator;
    private readonly IValidator<SearchPharmaceuticalFormsRequest> _validator = validator;

    /// <summary>
    /// Searches pharmaceutical forms by formulation name with pagination.
    /// </summary>
    /// <param name="name">Optional partial match filter for formulation name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="skip">Number of results to skip for pagination, defaults to 0.</param>
    /// <param name="take">Number of results to return, defaults to 20.</param>
    /// <response code="200">Returns a page of matching pharmaceutical forms.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchPharmaceuticalFormsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchPharmaceuticalFormsResponse>> Search(
        [FromQuery] string? name,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        SearchPharmaceuticalFormsRequest request = new(name, skip, take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchPharmaceuticalFormsQuery query = request.MapToQuery();
        ErrorOr<SearchPharmaceuticalFormsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext)
        );
    }
}


