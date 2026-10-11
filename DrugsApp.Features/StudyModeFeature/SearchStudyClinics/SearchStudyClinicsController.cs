using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.SearchStudyClinics;

/// <summary>
/// Query-string parameters of the study clinic search.
/// </summary>
/// <param name="Search">Optional case-insensitive contains filter on the clinic name.</param>
/// <param name="Skip">Paging offset.</param>
/// <param name="Take">Page size (1–40).</param>
public sealed record SearchStudyClinicsRequest(
    [FromQuery(Name = "search")] string? Search,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20);

/// <summary>
/// Query searching study clinics with paging.
/// </summary>
/// <param name="Request">Composed query-string request.</param>
/// <param name="CallerUserId">Calling admin user; private clinics of other users are excluded.</param>
public sealed record SearchStudyClinicsQuery(
    SearchStudyClinicsRequest Request,
    Guid CallerUserId) : IRequest<ErrorOr<StudyClinicListResponse>>;

/// <summary>
/// Lists study clinics for teachers and admins with an optional name filter.
/// </summary>
[ApiController]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class SearchStudyClinicsController(
    IMediator _mediator,
    IValidator<SearchStudyClinicsRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "SearchStudyClinics";

    /// <summary>
    /// Searches the public clinics and the caller's own private clinics, ordered by creation time descending, then by name.
    /// </summary>
    /// <param name="request">Search and paging parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns a page of study clinics with the total match count.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(StudyClinicListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StudyClinicListResponse>> Search(
        [FromQuery] SearchStudyClinicsRequest request,
        CancellationToken cancellationToken)
    {
        // run validation
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid callerUserId))
        {
            return Unauthorized();
        }

        // map to the query to send to the handler mediator
        SearchStudyClinicsQuery query = request.MapToQuery(callerUserId);
        ErrorOr<StudyClinicListResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
