using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.SupplementFeature.CheckSupplementGroupNameAvailability;

/// <summary>
/// Request model for supplement group name availability check.
/// </summary>
public sealed record CheckSupplementGroupNameAvailabilityRequest(
    string Name,
    Guid? ExcludeId);

/// <summary>
/// Query for checking supplement group name availability.
/// </summary>
public sealed record CheckSupplementGroupNameAvailabilityQuery(
    string Name,
    Guid? ExcludeId) : IRequest<ErrorOr<CheckSupplementGroupNameAvailabilityResponse>>;

/// <summary>
/// Response for supplement group name availability.
/// </summary>
public sealed record CheckSupplementGroupNameAvailabilityResponse(
    bool IsAvailable,
    Guid? ExistingId);

/// <summary>
/// Controller for supplement group name availability checks.
/// </summary>
[ApiController]
[Tags("SupplementGroups")]
[Route("api/supplement-groups")]
public sealed class CheckSupplementGroupNameAvailabilityController(
    IMediator mediator,
    IValidator<CheckSupplementGroupNameAvailabilityRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name for group name availability.
    /// </summary>
    public const string RouteName = "CheckSupplementGroupNameAvailability";

    /// <summary>
    /// Checks if a supplement group name is available.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <param name="excludeId">Optional group id to exclude.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Availability status.</response>
    /// <response code="400">Invalid query.</response>
    [HttpGet("validate-name-availability", Name = RouteName)]
    [ProducesResponseType(typeof(CheckSupplementGroupNameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CheckSupplementGroupNameAvailabilityResponse>> Check(
        [FromQuery(Name = "name")] string name,
        [FromQuery(Name = "exclude_id")] Guid? excludeId,
        CancellationToken cancellationToken)
    {
        CheckSupplementGroupNameAvailabilityRequest request = new(name, excludeId);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckSupplementGroupNameAvailabilityQuery query = request.MapToQuery();
        ErrorOr<CheckSupplementGroupNameAvailabilityResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
