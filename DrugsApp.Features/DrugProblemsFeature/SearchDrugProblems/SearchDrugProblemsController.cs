using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugProblemsFeature.SearchDrugProblems;

/// <summary>
/// Request model for searching drug problems.
/// </summary>
/// <param name="Skip">Number of items to skip.</param>
/// <param name="Take">Number of items to take.</param>
public sealed record SearchDrugProblemsRequest(
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 10);

/// <summary>
/// Query for searching drug problems scoped to the current user.
/// </summary>
/// <param name="UserId">Identifier of the current user.</param>
/// <param name="Skip">Number of items to skip.</param>
/// <param name="Take">Number of items to take.</param>
public sealed record SearchDrugProblemsQuery(
    Guid UserId,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchDrugProblemsResponse>>;

/// <summary>
/// Response payload for drug problem search.
/// </summary>
/// <param name="Items">Returned list items.</param>
/// <param name="TotalCount">Optional total count when available.</param>
/// <param name="HasMore">Indicates if more items exist.</param>
public sealed record SearchDrugProblemsResponse(
    IReadOnlyCollection<DrugProblemListItemDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Controller for searching user drug problems.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Drug Problems")]
[Route("api/drug-problems")]
public sealed class SearchDrugProblemsController(
    IMediator _mediator,
    IValidator<SearchDrugProblemsRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for searching drug problems.
    /// </summary>
    public const string RouteName = "SearchDrugProblems";

    /// <summary>
    /// Searches the current user's drug problems with paging metadata.
    /// </summary>
    /// <param name="request">Search request with paging.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchDrugProblemsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SearchDrugProblemsResponse>> Search(
        [FromQuery] SearchDrugProblemsRequest request,
        CancellationToken cancellationToken = default)
    {
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

        SearchDrugProblemsQuery query = request.MapToQuery(userId);
        ErrorOr<SearchDrugProblemsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<SearchDrugProblemsResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
