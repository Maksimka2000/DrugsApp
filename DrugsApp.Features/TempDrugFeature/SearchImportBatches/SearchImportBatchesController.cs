using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.SearchImportBatches;

/// <summary>
/// Query request model for searching import batches.
/// </summary>
public sealed record SearchImportBatchesRequest(
    string SortBy,
    string OrderBy,
    int Skip,
    int Take);

public sealed record SearchImportBatchesResponse(
    List<ImportBatchDto> Items,
    int? TotalCount,
    bool HasMore);


/// <summary>
/// Query model for searching import batches.
/// </summary>
public sealed record SearchImportBatchesQuery(
    string SortBy,
    string OrderBy,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchImportBatchesResponse>>;

/// <summary>
/// Controller for searching import batches history.
/// </summary>
[ApiController]
[Tags("TempDrugImport")]
[Route("api/temp-drug-import-batches")]
public sealed class SearchImportBatchesController(
    IMediator _mediator,
    IValidator<SearchImportBatchesRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Returns paged import history with has-more metadata.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<SearchImportBatchesResponse>> Search(
        [FromQuery(Name = "sort_by")] string sortBy = "created_at",
        [FromQuery(Name = "order_by")] string orderBy = "descending",
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var request = new SearchImportBatchesRequest(
            SortBy: sortBy,
            OrderBy: orderBy,
            Skip: skip,
            Take: take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchImportBatchesQuery query = request.MapToQuery();
        ErrorOr<SearchImportBatchesResponse> response = await _mediator.Send(query, cancellationToken);

        return response.Match(
            result => Ok(result),
            errors => errors.MapToProblem(HttpContext));
    }
}

