using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.CheckActiveSubstanceGroupNameAvailability;

/// <summary>
/// Request for validating active substance group name availability.
/// </summary>
public sealed record CheckActiveSubstanceGroupNameAvailabilityRequest(
    string Name,
    Guid? ExcludeId);

/// <summary>
/// Availability response for active substance group name.
/// </summary>
public sealed record ActiveSubstanceGroupNameAvailabilityResponse(
    bool IsAvailable,
    Guid? ConflictingGroupId);

/// <summary>
/// Query for checking active substance group name availability.
/// </summary>
public sealed record CheckActiveSubstanceGroupNameAvailabilityQuery(
    string Name,
    Guid? ExcludeId) : IRequest<ErrorOr<ActiveSubstanceGroupNameAvailabilityResponse>>;

/// <summary>
/// Controller for validating group name availability.
/// </summary>
[ApiController]
[Tags("Active Substance Groups")]
[Route("api/active-substance-groups/name-availability")]
public sealed class CheckActiveSubstanceGroupNameAvailabilityController(
    IMediator _mediator,
    IValidator<CheckActiveSubstanceGroupNameAvailabilityRequest> _validator) : ControllerBase
{
    public const string RouteName = "CheckActiveSubstanceGroupNameAvailability";

    /// <summary>
    /// Checks whether a group name is available.
    /// </summary>
    /// <param name="name">Group name to validate.</param>
    /// <param name="excludeId">Optional id to exclude when validating.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns availability result.</response>
    /// <response code="400">Invalid parameters supplied.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(ActiveSubstanceGroupNameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActiveSubstanceGroupNameAvailabilityResponse>> CheckAvailability(
        [FromQuery(Name = "name")] string name,
        [FromQuery(Name = "exclude_id")] Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        CheckActiveSubstanceGroupNameAvailabilityRequest request = new(name, excludeId);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckActiveSubstanceGroupNameAvailabilityQuery query = request.MapToQuery();
        ErrorOr<ActiveSubstanceGroupNameAvailabilityResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
