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

namespace DrugsApp.Features.FoodFeature.CheckFoodNameAvailability;

/// <summary>
/// Request model for food name availability.
/// </summary>
public sealed record CheckFoodNameAvailabilityRequest(
    string Name,
    Guid? ExcludeId);

/// <summary>
/// Query for checking food name availability.
/// </summary>
public sealed record CheckFoodNameAvailabilityQuery(
    string Name,
    System.Guid? ExcludeId) : IRequest<ErrorOr<CheckFoodNameAvailabilityResponse>>;

/// <summary>
/// Response describing food name availability.
/// </summary>
public sealed record CheckFoodNameAvailabilityResponse(
    bool IsAvailable,
    Guid? ConflictingResourceId);


/// <summary>
/// Controller for checking food name availability.
/// </summary>
[ApiController]
[Tags("Foods")]
[Route("api/foods/validate-name-availability")]
public sealed class CheckFoodNameAvailabilityController(
    IMediator mediator,
    IValidator<CheckFoodNameAvailabilityRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "CheckFoodNameAvailability";

    /// <summary>
    /// Checks whether a food name is available for create or update.
    /// </summary>
    /// <param name="name">Name to validate.</param>
    /// <param name="excludeId">Optional food id to exclude from uniqueness check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns availability result.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(CheckFoodNameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CheckFoodNameAvailabilityResponse>> Check(
        [FromQuery(Name = "name")] string name,
        [FromQuery(Name = "exclude_id")] Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var request = new CheckFoodNameAvailabilityRequest(name, excludeId);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckFoodNameAvailabilityQuery query = request.MapToQuery();
        ErrorOr<CheckFoodNameAvailabilityResponse> response = await mediator.Send(query, cancellationToken);

        return response.Match(
            availability => Ok(availability),
            errors => errors.MapToProblem(HttpContext));
    }
}
