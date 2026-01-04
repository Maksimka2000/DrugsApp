using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugFeature.SearchDrugSimilarities;

/// <summary>
/// Request model for searching drug similarities.
/// </summary>
public sealed record SearchDrugSimilaritiesRequest(
    string CommercialName,
    string TypeOfProduct,
    Guid[]? ActiveSubstanceIds,
    string? AtcCode,
    string? Dosage,
    string? Leaflet,
    string? EducationalMaterials,
    Guid[]? RouteOfAdministrationIds,
    Guid[]? PharmaceuticalFormIds);

/// <summary>
/// Response model for searching drug similarities.
/// </summary>
public sealed record SearchDrugSimilaritiesResponse(
    IReadOnlyCollection<DrugSimilarityDto> Items);

/// <summary>
/// Data transfer object representing a drug similarity.
/// </summary>
public sealed record DrugSimilarityDto(
    int MatchScorePercent,
    string SimilarityClassification,
    bool IsMergeAllowed,
    bool IsUpdateAllowed,
    DrugSimilarityRowDto Row);
    
/// <summary>
/// Data transfer object representing a drug similarity row.
/// </summary>
public sealed record DrugSimilarityRowDto(
    Guid DrugId,
    string CommercialName,
    string TypeOfProduct,
    IReadOnlyCollection<LookupDto> ActiveSubstances,
    DrugFormDto DrugForm);

/// <summary>
/// Query for searching drug similarities.
/// </summary>
public sealed record SearchDrugSimilaritiesQuery(
    SearchDrugSimilaritiesRequest Request)
    : IRequest<ErrorOr<SearchDrugSimilaritiesResponse>>;



/// <summary>
/// Controller for searching drug similarities.
/// </summary>
/// <param name="_mediator">MediatR mediator.</param>
/// <param name="_validator">FluentValidation validator.</param>
[ApiController]
[Tags("Drugs")]
[Route("api/drugs")]
public sealed class SearchDrugSimilaritiesController(
    IMediator _mediator,
    IValidator<SearchDrugSimilaritiesRequest> _validator) : ControllerBase
{

    /// <summary>
    /// Searches drug similarities.
    /// </summary>
    /// <param name="request">Search drug similarities request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search drug similarities response.</returns>
    /// <response code="200">Returns the search drug similarities response.</response>
    /// <response code="400">Invalid request payload.</response>
    [HttpPost("similarities")]
    [ProducesResponseType(typeof(SearchDrugSimilaritiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchDrugSimilaritiesResponse>> Search(
        [FromBody] SearchDrugSimilaritiesRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchDrugSimilaritiesQuery query = request.MapToQuery();
        ErrorOr<SearchDrugSimilaritiesResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

