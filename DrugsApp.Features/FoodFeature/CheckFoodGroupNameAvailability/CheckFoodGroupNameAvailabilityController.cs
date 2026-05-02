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

namespace DrugsApp.Features.FoodFeature.CheckFoodGroupNameAvailability;

/// <summary>
/// Request model for food group name availability.
/// </summary>
public sealed record CheckFoodGroupNameAvailabilityRequest(
    string Name,
    Guid? ExcludeId);

/// <summary>
/// Query for checking food group name availability.
/// </summary>
public sealed record CheckFoodGroupNameAvailabilityQuery(
    string Name,
    Guid? ExcludeId) : IRequest<ErrorOr<CheckFoodGroupNameAvailabilityResponse>>;

/// <summary>
/// Response describing food group name availability.
/// </summary>
public sealed record CheckFoodGroupNameAvailabilityResponse(
    bool IsAvailable,
    Guid? ConflictingResourceId);

/// <summary>
/// Controller for checking food group name availability.
/// </summary>
[ApiController]
[Tags("Food Groups")]
[Route("api/food-groups/validate-name-availability")]
public sealed class CheckFoodGroupNameAvailabilityController(
    IMediator mediator,
    IValidator<CheckFoodGroupNameAvailabilityRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "CheckFoodGroupNameAvailability";

    /// <summary>
    /// Checks whether a food group name is available for create or update flows.
    /// </summary>
    /// <param name="name">Name to validate.</param>
    /// <param name="excludeId">Optional food group id to exclude from check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns availability result.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(CheckFoodGroupNameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CheckFoodGroupNameAvailabilityResponse>> Check(
        [FromQuery(Name = "name")] string name,
        [FromQuery(Name = "exclude_id")] Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var request = new CheckFoodGroupNameAvailabilityRequest(name, excludeId);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckFoodGroupNameAvailabilityQuery query = request.MapToQuery();
        ErrorOr<CheckFoodGroupNameAvailabilityResponse> response = await mediator.Send(query, cancellationToken);

        return response.Match(
            availability => Ok(availability),
            errors => errors.MapToProblem(HttpContext));
    }
}
