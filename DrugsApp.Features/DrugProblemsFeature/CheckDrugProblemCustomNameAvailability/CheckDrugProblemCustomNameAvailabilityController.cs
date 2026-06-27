using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemCustomNameAvailability;

/// <summary>
/// Request model for checking custom drug problem name availability.
/// </summary>
/// <param name="CustomDrugName">Custom drug name to validate.</param>
/// <param name="ExcludeId">Optional problem id to exclude from the check.</param>
public sealed record CheckDrugProblemCustomNameAvailabilityRequest(
    [FromQuery(Name = "custom_drug_name")] string CustomDrugName,
    [FromQuery(Name = "exclude_id")] Guid? ExcludeId);

/// <summary>
/// Query for custom name availability check.
/// </summary>
/// <param name="UserId">Current user identifier.</param>
/// <param name="CustomDrugName">Normalized custom drug name.</param>
/// <param name="ExcludeId">Optional problem id to exclude.</param>
public sealed record CheckDrugProblemCustomNameAvailabilityQuery(
    Guid UserId,
    string CustomDrugName,
    Guid? ExcludeId) : IRequest<ErrorOr<DrugProblemCustomNameAvailabilityDto>>;

/// <summary>
/// Availability response.
/// </summary>
/// <param name="IsAvailable">Indicates whether the name is free to use.</param>
/// <param name="ConflictingResourceId">Id of the conflicting record when unavailable.</param>
public sealed record DrugProblemCustomNameAvailabilityDto(bool IsAvailable, Guid? ConflictingResourceId);

/// <summary>
/// Controller for checking custom name availability.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Drug Problems")]
[Route("api/drug-problems/custom-name-availability")]
public sealed class CheckDrugProblemCustomNameAvailabilityController(
    IMediator _mediator,
    IValidator<CheckDrugProblemCustomNameAvailabilityRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for custom name availability.
    /// </summary>
    public const string RouteName = "CheckDrugProblemCustomNameAvailability";

    /// <summary>
    /// Checks availability of a custom drug problem name for the current user.
    /// </summary>
    /// <param name="request">Query string payload with name and optional exclude id.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(DrugProblemCustomNameAvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DrugProblemCustomNameAvailabilityDto>> Check(
        [FromQuery] CheckDrugProblemCustomNameAvailabilityRequest request,
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

        CheckDrugProblemCustomNameAvailabilityQuery query = request.MapToQuery(userId);
        ErrorOr<DrugProblemCustomNameAvailabilityDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<DrugProblemCustomNameAvailabilityDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
