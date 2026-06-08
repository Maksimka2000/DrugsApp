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

namespace DrugsApp.Features.SupportFeature.SearchAdminFaqEntries;

/// <summary>
/// Request model for admin FAQ search parameters.
/// </summary>
public sealed record SearchAdminFaqEntriesRequest(
    [FromQuery(Name = "question_query")] string? QuestionQuery,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20);

/// <summary>
/// Response containing admin FAQ items and paging metadata.
/// </summary>
public sealed record SearchAdminFaqEntriesResponse(
    IReadOnlyCollection<AdminFaqEntryDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Query for searching admin FAQ entries.
/// </summary>
public sealed record SearchAdminFaqEntriesQuery(
    string? QuestionQuery,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchAdminFaqEntriesResponse>>;

/// <summary>
/// Controller for the admin support FAQ list endpoint.
/// </summary>
[ApiController]
[Tags("Support")]
[Route("api/support/admin/faqs")]
public sealed class SearchAdminFaqEntriesController(
    IMediator mediator,
    IValidator<SearchAdminFaqEntriesRequest> validator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IValidator<SearchAdminFaqEntriesRequest> _validator = validator;

    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "SearchAdminFaqEntries";

    /// <summary>
    /// Searches admin FAQ entries with optional question filtering and pagination.
    /// </summary>
    /// <param name="request">Admin FAQ query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns a page of admin FAQ entries.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchAdminFaqEntriesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchAdminFaqEntriesResponse>> Search(
        [FromQuery] SearchAdminFaqEntriesRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchAdminFaqEntriesQuery query = request.MapToQuery();
        ErrorOr<SearchAdminFaqEntriesResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
