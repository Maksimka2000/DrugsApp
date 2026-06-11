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

namespace DrugsApp.Features.DrugFeature.SearchDrugForms;

/// <summary>
/// Query request model for searching drug forms.
/// </summary>
public sealed record SearchDrugFormsRequest(
    string? SearchQuery,
    string SortBy,
    string OrderBy,
    int Skip,
    int Take);

/// <summary>
/// Query model for searching drug forms.
/// </summary>
public sealed record SearchDrugFormsQuery(
    string? SearchQuery,
    string SortBy,
    string OrderBy,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchDrugFormsResponse>>;

/// <summary>
/// Response containing drug form search rows and paging metadata.
/// </summary>
public sealed record SearchDrugFormsResponse(
    List<SearchDrugFormDto> Items,
    int? TotalCount,
    bool HasMore,
    SearchDrugFormsMetadata Metadata);

/// <summary>
/// Provides metadata describing how the drug form search query was executed.
/// </summary>
public sealed record SearchDrugFormsMetadata(
    string? MatchedField,
    bool UsedFallback,
    IReadOnlyList<string>? AttemptedFields,
    string Message);

/// <summary>
/// Controller for searching drug form candidates used by client pick-lists.
/// </summary>
[ApiController]
[Tags("Drug Forms")]
[Route("api/drug-forms")]
public sealed class SearchDrugFormsController(
    IMediator _mediator,
    IValidator<SearchDrugFormsRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Searches drug form candidates using prioritized fallback matching and paging metadata.
    /// </summary>
    /// <param name="searchQuery">Optional mixed-field search query.</param>
    /// <param name="sortBy">Sort field (commercial_name, created_at, updated_at).</param>
    /// <param name="orderBy">Sort order (ascending/descending).</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns a page of drug form candidates.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet]
    public async Task<ActionResult<SearchDrugFormsResponse>> Search(
        [FromQuery(Name = "search_query")] string? searchQuery,
        CancellationToken cancellationToken,
        [FromQuery(Name = "sort_by")] string sortBy = "commercial_name",
        [FromQuery(Name = "order_by")] string orderBy = "ascending",
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10)
    {
        SearchDrugFormsRequest request = new(
            SearchQuery: searchQuery,
            SortBy: sortBy,
            OrderBy: orderBy,
            Skip: skip,
            Take: take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchDrugFormsQuery query = request.MapToQuery();

        ErrorOr<SearchDrugFormsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
