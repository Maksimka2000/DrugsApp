using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthConditionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthConditionFeature.SearchUserHealthConditions;

/// <summary>
/// Request model for searching user health conditions.
/// </summary>
public sealed record SearchUserHealthConditionsRequest(
    string? DisplayNameQuery,
    int Skip,
    int Take);

/// <summary>
/// Response model for user health condition search.
/// </summary>
public sealed record SearchUserHealthConditionsResponse(
    List<UserHealthConditionDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Query contract for searching user health conditions.
/// </summary>
public sealed record SearchUserHealthConditionsQuery(
    Guid UserId,
    string? DisplayNameQuery,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchUserHealthConditionsResponse>>;

/// <summary>
/// Controller for searching user health conditions.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("User Health Conditions")]
[Route("api/user-health-conditions")]
public sealed class SearchUserHealthConditionsController(
    IMediator _mediator,
    IValidator<SearchUserHealthConditionsRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for searching user health conditions.
    /// </summary>
    public const string RouteName = "SearchUserHealthConditions";

    /// <summary>
    /// Searches user health conditions for the current user.
    /// </summary>
    /// <param name="displayNameQuery">Optional display name filter.</param>
    /// <param name="skip">Paging offset.</param>
    /// <param name="take">Paging size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paged list of user health conditions.</response>
    /// <response code="400">Invalid query parameters.</response>
    /// <response code="401">User not authenticated.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchUserHealthConditionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SearchUserHealthConditionsResponse>> Search(
        [FromQuery(Name = "display_name_query")] string? displayNameQuery,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10,
        CancellationToken cancellationToken = default)
    {
        SearchUserHealthConditionsRequest request = new(
            displayNameQuery,
            skip,
            take);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        SearchUserHealthConditionsQuery query = request.MapToQuery(userId);
        ErrorOr<SearchUserHealthConditionsResponse> result =
            await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
