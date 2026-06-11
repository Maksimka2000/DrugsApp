using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.MealTimeManagementFeature.CheckMealTimeAvailability;

/// <summary>
/// Request model for checking meal time availability.
/// </summary>
public sealed record CheckMealTimeAvailabilityRequest(
    string? MealName,
    TimeOnly? IntakeTime,
    Guid? ExcludeId);

/// <summary>
/// Query for checking meal time availability.
/// </summary>
public sealed record CheckMealTimeAvailabilityQuery(
    Guid UserId,
    string? MealName,
    TimeOnly? IntakeTime,
    Guid? ExcludeId) : IRequest<ErrorOr<MealTimeAvailabilityResponse>>;

/// <summary>
/// Controller for checking meal time availability.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Meal Times")]
[Route("api/meal-times")]
public sealed class CheckMealTimeAvailabilityController(
    IMediator _mediator,
    IValidator<CheckMealTimeAvailabilityRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for checking meal time availability.
    /// </summary>
    public const string RouteName = "CheckMealTimeAvailability";

    /// <summary>
    /// Checks whether a meal name and/or intake time are available for the current user.
    /// </summary>
    [HttpGet("availability", Name = RouteName)]
    [ProducesResponseType(typeof(MealTimeAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MealTimeAvailabilityResponse>> Check(
        [FromQuery(Name = "meal_name")] string? mealName,
        [FromQuery(Name = "intake_time")] TimeOnly? intakeTime,
        [FromQuery(Name = "exclude_id")] Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        CheckMealTimeAvailabilityRequest request = new(mealName, intakeTime, excludeId);

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

        CheckMealTimeAvailabilityQuery query = request.MapToQuery(userId);
        ErrorOr<MealTimeAvailabilityResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<MealTimeAvailabilityResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
