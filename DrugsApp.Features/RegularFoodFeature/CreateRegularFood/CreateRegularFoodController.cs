using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularFoodFeature.GetRegularFood;
using DrugsApp.Features.RegularFoodFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularFoodFeature.CreateRegularFood;

/// <summary>
/// Request model for creating a regular food schedule.
/// </summary>
public sealed record CreateRegularFoodRequest(
    Guid FoodId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);

/// <summary>
/// Command for creating a regular food schedule.
/// </summary>
public sealed record CreateRegularFoodCommand(
    Guid UserId,
    CreateRegularFoodRequest Request) : IRequest<ErrorOr<CreateRegularFoodResponse>>;

/// <summary>
/// Response containing the created regular food schedule.
/// </summary>
public sealed record CreateRegularFoodResponse(RegularFoodListItemDto Item);

/// <summary>
/// Controller for creating regular food schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Regular Food")]
[Route("api/regular-food")]
public sealed class CreateRegularFoodController(
    IMediator _mediator,
    IValidator<CreateRegularFoodRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for creating a regular food schedule.
    /// </summary>
    public const string RouteName = "CreateRegularFood";

    /// <summary>
    /// Creates a new regular food schedule for the authenticated user.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(CreateRegularFoodResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateRegularFoodResponse>> Create(
        [FromBody] CreateRegularFoodRequest request,
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

        CreateRegularFoodCommand command = request.MapToCommand(userId);
        ErrorOr<CreateRegularFoodResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<CreateRegularFoodResponse>>(
            response => CreatedAtRoute(
                GetRegularFoodController.RouteName,
                new { id = response.Item.Id },
                response),
            errors => errors.MapToProblem(HttpContext));
    }
}
