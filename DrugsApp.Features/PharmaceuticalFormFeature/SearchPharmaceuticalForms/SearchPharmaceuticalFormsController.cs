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
    string? FormulationName,
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
    string? FormulationName,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchPharmaceuticalFormsResponse>>;

/// <summary>
/// Controller for searching pharmaceutical forms with filtering and pagination.
/// </summary>
[ApiController]
[Tags("Pharmaceutical Forms")]
[Route("api/pharmaceutical-forms")]
public sealed class SearchPharmaceuticalFormsController(
    IMediator _mediator,
    IValidator<SearchPharmaceuticalFormsRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "SearchPharmaceuticalForms";

    /// <summary>
    /// Searches pharmaceutical forms by formulation name with pagination.
    /// </summary>
    /// <param name="formulationName">Optional partial match filter for formulation name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="skip">Number of results to skip for pagination, defaults to 0.</param>
    /// <param name="take">Number of results to return, defaults to 20.</param>
    /// <response code="200">Returns a page of matching pharmaceutical forms.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchPharmaceuticalFormsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchPharmaceuticalFormsResponse>> Search(
        [FromQuery(Name = "formulation_name")] string? formulationName,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10,
        CancellationToken cancellationToken = default)
    {
        SearchPharmaceuticalFormsRequest request = new(formulationName, skip, take);

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


