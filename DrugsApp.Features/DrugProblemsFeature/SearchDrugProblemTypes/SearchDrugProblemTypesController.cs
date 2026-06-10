using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugProblemsFeature.SearchDrugProblemTypes;

/// <summary>
/// Request model for searching drug problem types.
/// </summary>
public sealed record SearchDrugProblemTypesRequest(
    [FromQuery(Name = "problem_type_name_query")] string? ProblemTypeNameQuery,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 10);

/// <summary>
/// Query for searching drug problem types.
/// </summary>
public sealed record SearchDrugProblemTypesQuery(
    string? ProblemTypeNameQuery,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchDrugProblemTypesResponse>>;

/// <summary>
/// Response payload for drug problem types search.
/// </summary>
public sealed record SearchDrugProblemTypesResponse(
    IReadOnlyCollection<DrugProblemTypeDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Controller for searching drug problem types.
/// </summary>
[ApiController]
[Authorize]
[Tags("Drug Problem Types")]
[Route("api/drug-problem-types")]
public sealed class SearchDrugProblemTypesController(
    IMediator _mediator,
    IValidator<SearchDrugProblemTypesRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for searching drug problem types.
    /// </summary>
    public const string RouteName = "SearchDrugProblemTypes";

    /// <summary>
    /// Searches drug problem types for autocomplete and admin list.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(SearchDrugProblemTypesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SearchDrugProblemTypesResponse>> Search(
        [FromQuery] SearchDrugProblemTypesRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchDrugProblemTypesQuery query = request.MapToQuery();
        ErrorOr<SearchDrugProblemTypesResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<SearchDrugProblemTypesResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
