using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.SearchTempDrugs;

/// <summary>
/// Query request model for searching temporary drugs.
/// </summary>
public sealed record SearchTempDrugsRequest(
    string? CommercialName,
    List<string>? Statuses,
    List<string>? TypeOfProducts,
    string SortBy,
    string OrderBy,
    int Skip,
    int Take);

/// <summary>
/// Query model for searching temporary drugs.
/// </summary>
public sealed record SearchTempDrugsQuery(
    string? CommercialName,
    List<string>? Statuses,
    List<string>? TypeOfProducts,
    string SortBy,
    string OrderBy,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchTempDrugsResponse>>;

/// <summary>
/// Response containing temporary drugs list and paging metadata.
/// </summary>
public sealed record SearchTempDrugsResponse(
    List<TempDrugDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Controller for searching and listing temporary drugs.
/// </summary>
[ApiController]
[Tags("Temp Drugs")]
[Route("api/temp-drugs")]
public sealed class SearchTempDrugsController(
    IMediator _mediator,
    IValidator<SearchTempDrugsRequest> _validator) : ControllerBase
{

    /// <summary>
    /// Searches temporary drugs with filters, sorting, and pagination.
    /// </summary>
    /// <param name="commercialName">Optional commercial name search term.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="statuses">Optional repeated statuses filter set.</param>
    /// <param name="typeOfProducts">Optional repeated type-of-product filter set.</param>
    /// <param name="sortBy">Sort field (created_at, updated_at, commercial_name, identifier).</param>
    /// <param name="orderBy">Sort order (ascending/descending).</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <response code="200">Returns a page of temporary drugs.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet]
    public async Task<ActionResult<SearchTempDrugsResponse>> Search(
        [FromQuery(Name = "commercial_name")] string? commercialName,
        CancellationToken cancellationToken,
        [FromQuery(Name = "statuses")] List<string>? statuses,
        [FromQuery(Name = "type_of_drug")] List<string>? typeOfProducts,
        [FromQuery(Name = "sort_by")] string sortBy = "created_at",
        [FromQuery(Name = "order_by")] string orderBy = "descending",
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10)
    {
        var request = new SearchTempDrugsRequest(
            CommercialName: commercialName,
            Statuses: statuses,
            TypeOfProducts: typeOfProducts,
            SortBy: sortBy,
            OrderBy: orderBy,
            Skip: skip,
            Take: take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchTempDrugsQuery query = request.MapToQuery();
        ErrorOr<SearchTempDrugsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}


