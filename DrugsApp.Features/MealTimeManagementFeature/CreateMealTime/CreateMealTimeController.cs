using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.MealTimeManagementFeature.RetrieveMealTime;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.MealTimeManagementFeature.CreateMealTime;

/// <summary>
/// Request model for creating a meal time.
/// </summary>
public sealed record CreateMealTimeRequest(string MealName, string IntakeTime);

/// <summary>
/// Command for creating a meal time.
/// </summary>
public sealed record CreateMealTimeCommand(
    Guid UserId,
    CreateMealTimeRequest Request) : IRequest<ErrorOr<MealTimeDto>>;

/// <summary>
/// Controller for creating meal times.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Meal Times")]
[Route("api/meal-times")]
public sealed class CreateMealTimeController(
    IMediator _mediator,
    IValidator<CreateMealTimeRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for creating a meal time.
    /// </summary>
    public const string RouteName = "CreateMealTime";

    /// <summary>
    /// Creates a meal time for the current user.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(MealTimeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MealTimeDto>> Create(
        [FromBody] CreateMealTimeRequest request,
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

        CreateMealTimeCommand command = request.MapToCommand(userId);
        ErrorOr<MealTimeDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<MealTimeDto>>(
            response => CreatedAtRoute(
                routeName: RetrieveMealTimeController.RouteName,
                routeValues: new { id = response.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
