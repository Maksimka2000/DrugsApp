using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.SupplementFeature.CheckSupplementNameAvailability;

/// <summary>
/// Request model for supplement name availability check.
/// </summary>
public sealed record CheckSupplementNameAvailabilityRequest(
    string Name,
    Guid? ExcludeId);

/// <summary>
/// Query for checking supplement name availability.
/// </summary>
public sealed record CheckSupplementNameAvailabilityQuery(
    string Name,
    Guid? ExcludeId) : IRequest<ErrorOr<CheckSupplementNameAvailabilityResponse>>;

/// <summary>
/// Response for supplement name availability.
/// </summary>
public sealed record CheckSupplementNameAvailabilityResponse(
    bool IsAvailable,
    Guid? ExistingId);

/// <summary>
/// Controller for supplement name availability checks.
/// </summary>
[ApiController]
[Tags("Supplements")]
[Route("api/supplements")]
public sealed class CheckSupplementNameAvailabilityController(
    IMediator mediator,
    IValidator<CheckSupplementNameAvailabilityRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name for name availability check.
    /// </summary>
    public const string RouteName = "CheckSupplementNameAvailability";

    /// <summary>
    /// Checks if a supplement name is available.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <param name="excludeId">Optional supplement id to exclude.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Availability status.</response>
    /// <response code="400">Invalid query.</response>
    [HttpGet("validate-name-availability", Name = RouteName)]
    [ProducesResponseType(typeof(CheckSupplementNameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CheckSupplementNameAvailabilityResponse>> Check(
        [FromQuery(Name = "name")] string name,
        [FromQuery(Name = "exclude_id")] Guid? excludeId,
        CancellationToken cancellationToken)
    {
        CheckSupplementNameAvailabilityRequest request = new(name, excludeId);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckSupplementNameAvailabilityQuery query = request.MapToQuery();
        ErrorOr<CheckSupplementNameAvailabilityResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
