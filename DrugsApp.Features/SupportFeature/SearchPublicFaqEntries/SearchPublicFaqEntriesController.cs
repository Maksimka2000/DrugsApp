using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.SupportFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.SupportFeature.SearchPublicFaqEntries;

/// <summary>
/// Request model for public FAQ search parameters.
/// </summary>
public sealed record SearchPublicFaqEntriesRequest(
    [FromQuery(Name = "question_query")] string? QuestionQuery,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20);

/// <summary>
/// Response containing public FAQ items and paging metadata.
/// </summary>
public sealed record SearchPublicFaqEntriesResponse(
    IReadOnlyCollection<FaqEntryDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Query for searching public FAQ entries.
/// </summary>
public sealed record SearchPublicFaqEntriesQuery(
    string? QuestionQuery,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchPublicFaqEntriesResponse>>;

/// <summary>
/// Controller for the public support FAQ list endpoint.
/// </summary>
[ApiController]
[Tags("Support")]
[Route("api/support/faqs")]
public sealed class SearchPublicFaqEntriesController(
    IMediator mediator,
    IValidator<SearchPublicFaqEntriesRequest> validator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IValidator<SearchPublicFaqEntriesRequest> _validator = validator;

    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "SearchPublicFaqEntries";

    /// <summary>
    /// Searches public FAQ entries with optional question filtering and pagination.
    /// </summary>
    /// <param name="request">Public FAQ query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns a page of public FAQ entries.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchPublicFaqEntriesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchPublicFaqEntriesResponse>> Search(
        [FromQuery] SearchPublicFaqEntriesRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchPublicFaqEntriesQuery query = request.MapToQuery();
        ErrorOr<SearchPublicFaqEntriesResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
