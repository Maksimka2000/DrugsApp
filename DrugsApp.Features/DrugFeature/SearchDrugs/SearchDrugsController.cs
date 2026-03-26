using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugFeature.Shared;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.DrugFeature.SearchDrugs;

/// <summary>
/// Query request model for searching drugs.
/// </summary>
public sealed record SearchDrugsRequest(
    string? SearchQuery,
    List<string>? TypeOfDrugs,
    string SortBy,
    string OrderBy,
    int Skip,
    int Take);

/// <summary>
/// Query model for searching drugs.
/// </summary>
public sealed record SearchDrugsQuery(
    string? SearchQuery,
    List<string>? TypeOfDrugs,
    string SortBy,
    string OrderBy,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchDrugsResponse>>;

/// <summary>
/// Response containing drug list and paging metadata.
/// </summary>
public sealed record SearchDrugsResponse(
    List<DrugDto> Items,
    int? TotalCount,
    bool HasMore,
    SearchDrugsMetadata Metadata);

/// <summary>
/// Provides metadata describing how the search query was executed.
/// </summary>
public sealed record SearchDrugsMetadata(
    string? MatchedField,
    bool UsedFallback,
    IReadOnlyList<string>? AttemptedFields,
    string Message);


/// <summary>
/// Controller for searching and listing drugs.
/// </summary>
[ApiController]
[Tags("Drugs")]
[Route("api/drugs")]
public sealed class SearchDrugsController(
    IMediator _mediator,
    IValidator<SearchDrugsRequest> _validator) : ControllerBase
{


    /// <summary>
    /// Searches drugs using mixed-field fuzzy query and paging metadata.
    /// </summary>
    /// <param name="searchQuery">Optional mixed-field search query.</param>
    /// <param name="typeOfDrug">Optional repeated filter set for drug type.</param>
    /// <param name="sortBy">Sort field (commercial_name, created_at, updated_at).</param>
    /// <param name="orderBy">Sort order (ascending/descending).</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns a page of drugs.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet]
    public async Task<ActionResult<SearchDrugsResponse>> Search(
        [FromQuery(Name = "search_query")] string? searchQuery,
        CancellationToken cancellationToken,
        [FromQuery(Name = "type_of_drug")] List<string>? typeOfDrug,
        [FromQuery(Name = "sort_by")] string sortBy = "commercial_name",
        [FromQuery(Name = "order_by")] string orderBy = "ascending",
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10
        )
    {
        
        var request = new SearchDrugsRequest(
            SearchQuery: searchQuery,
            TypeOfDrugs: typeOfDrug,
            SortBy: sortBy,
            OrderBy: orderBy,
            Skip: skip,
            Take: take);

        ValidationResult? validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchDrugsQuery query = request.MapToQuery();

        ErrorOr<SearchDrugsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}



