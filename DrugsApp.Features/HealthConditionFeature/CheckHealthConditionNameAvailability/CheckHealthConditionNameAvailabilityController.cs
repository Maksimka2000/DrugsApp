using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Enums;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthConditionFeature.CheckHealthConditionNameAvailability;

/// <summary>
/// Request model for checking catalog health condition name availability.
/// </summary>
public sealed record CheckHealthConditionNameAvailabilityRequest(
    string HealthConditionName,
    Guid? ExcludeId);

/// <summary>
/// Response indicating whether the name is available.
/// </summary>
public sealed record CheckHealthConditionNameAvailabilityResponse(
    bool IsAvailable,
    Guid? ConflictingResourceId);

/// <summary>
/// Query for checking catalog health condition name availability.
/// </summary>
public sealed record CheckHealthConditionNameAvailabilityQuery(
    string NormalizedHealthConditionName,
    Guid? ExcludeId) : IRequest<ErrorOr<CheckHealthConditionNameAvailabilityResponse>>;

/// <summary>
/// Controller for checking catalog health condition name availability.
/// </summary>
[ApiController]
[Authorize(Policy = nameof(Permission.ManageOther))]
[Tags("Health Conditions")]
[Route("api/health-conditions/name-availability")]
public sealed class CheckHealthConditionNameAvailabilityController(
    IMediator _mediator,
    IValidator<CheckHealthConditionNameAvailabilityRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for name availability endpoint.
    /// </summary>
    public const string RouteName = "CheckHealthConditionNameAvailability";

    /// <summary>
    /// Checks whether a catalog health condition name is available.
    /// </summary>
    /// <param name="healthConditionName">Name to check.</param>
    /// <param name="excludeId">Optional id to exclude when updating.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Availability result.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(CheckHealthConditionNameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CheckHealthConditionNameAvailabilityResponse>> Check(
        [FromQuery(Name = "health_condition_name")] string healthConditionName,
        [FromQuery(Name = "exclude_id")] Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        CheckHealthConditionNameAvailabilityRequest request = new(
            healthConditionName,
            excludeId);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckHealthConditionNameAvailabilityQuery query = request.MapToQuery();
        ErrorOr<CheckHealthConditionNameAvailabilityResponse> result =
            await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
