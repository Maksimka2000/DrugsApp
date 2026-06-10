using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemTypeNameAvailability;

/// <summary>
/// Request model for checking drug problem type name availability.
/// </summary>
public sealed record CheckDrugProblemTypeNameAvailabilityRequest(
    [FromQuery(Name = "problem_type_name")] string ProblemTypeName,
    [FromQuery(Name = "exclude_id")] Guid? ExcludeId);

/// <summary>
/// Query for checking drug problem type name availability.
/// </summary>
public sealed record CheckDrugProblemTypeNameAvailabilityQuery(
    string ProblemTypeName,
    Guid? ExcludeId) : IRequest<ErrorOr<DrugProblemTypeNameAvailabilityDto>>;

/// <summary>
/// Response for type name availability.
/// </summary>
public sealed record DrugProblemTypeNameAvailabilityDto(
    bool IsAvailable,
    Guid? ConflictingResourceId);

/// <summary>
/// Controller for checking drug problem type name availability.
/// </summary>
[ApiController]
[Authorize(Policy = "ManageOther")]
[Tags("Drug Problem Types")]
[Route("api/drug-problem-types/name-availability")]
public sealed class CheckDrugProblemTypeNameAvailabilityController(
    IMediator _mediator,
    IValidator<CheckDrugProblemTypeNameAvailabilityRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for drug problem type name availability.
    /// </summary>
    public const string RouteName = "CheckDrugProblemTypeNameAvailability";

    /// <summary>
    /// Checks whether a drug problem type name is available.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(DrugProblemTypeNameAvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DrugProblemTypeNameAvailabilityDto>> Check(
        [FromQuery] CheckDrugProblemTypeNameAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckDrugProblemTypeNameAvailabilityQuery query = request.MapToQuery();
        ErrorOr<DrugProblemTypeNameAvailabilityDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<DrugProblemTypeNameAvailabilityDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
