using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.FoodFeature.UpdateFood;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.FoodFeature.AddFood;

/// <summary>
/// Request model for creating a food.
/// </summary>
public sealed record AddFoodRequest(
    string Name,
    IReadOnlyCollection<string>? Aliases);

/// <summary>
/// Controller for creating foods.
/// </summary>
[ApiController]
[Tags("Foods")]
[Route("api/foods")]
public sealed class AddFoodController(
    IMediator mediator,
    IValidator<AddFoodRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "AddFood";

    /// <summary>
    /// Creates a new food with canonical name and optional aliases.
    /// </summary>
    /// <param name="request">Food creation payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Returns the created food.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="409">Food name already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreatedFoodResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedFoodResponse>> Create(
        [FromBody] AddFoodRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddFoodCommand command = request.MapToCommand();
        ErrorOr<CreatedFoodResponse> response = await mediator.Send(command, cancellationToken);

        return response.Match(
            created => CreatedAtRoute(
                routeName: UpdateFoodController.RouteName,
                routeValues: new { id = created.Food.Id },
                value: created),
            errors => errors.MapToProblem(HttpContext));
    }
}
