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

namespace DrugsApp.Features.MealTimeManagementFeature.UpdateMealTime;

/// <summary>
/// Request model for partially updating a meal time.
/// </summary>
public sealed record UpdateMealTimeRequest(string? MealName, TimeOnly? IntakeTime);

/// <summary>
/// Command for updating a meal time.
/// </summary>
public sealed record UpdateMealTimeCommand(
    Guid UserId,
    Guid MealTimeId,
    UpdateMealTimeRequest Request) : IRequest<ErrorOr<MealTimeDto>>;

/// <summary>
/// Controller for updating meal times.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Meal Times")]
[Route("api/meal-times")]
public sealed class UpdateMealTimeController(
    IMediator _mediator,
    IValidator<UpdateMealTimeRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for updating a meal time.
    /// </summary>
    public const string RouteName = "UpdateMealTime";

    /// <summary>
    /// Partially updates a meal time owned by the current user.
    /// </summary>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(MealTimeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MealTimeDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateMealTimeRequest request,
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

        UpdateMealTimeCommand command = request.MapToCommand(userId, id);
        ErrorOr<MealTimeDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<MealTimeDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
