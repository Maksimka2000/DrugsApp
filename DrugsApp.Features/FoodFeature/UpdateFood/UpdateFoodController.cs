using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DrugsApp.Features.FoodFeature.Shared;

namespace DrugsApp.Features.FoodFeature.UpdateFood;



/// <summary>
/// Request model for updating a food.
/// </summary>
public sealed record UpdateFoodRequest(
    string? Name,
    IReadOnlyCollection<string>? Aliases);

/// <summary>
/// Command for updating a food aggregate.
/// </summary>
public sealed record UpdateFoodCommand(
    Guid Id,
    UpdateFoodRequest Request) : IRequest<ErrorOr<UpdatedFoodResponse>>;

/// <summary>
/// Response containing updated food details.
/// </summary>
public sealed record UpdatedFoodResponse(
    FoodDto Food);




/// <summary>
/// Controller for updating foods.
/// </summary>
[ApiController]
[Tags("Foods")]
[Route("api/foods")]
public sealed class UpdateFoodController(
    IMediator mediator,
    IValidator<UpdateFoodRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "UpdateFood";

    /// <summary>
    /// Updates a food name or aliases.
    /// </summary>
    /// <param name="id">Food identifier.</param>
    /// <param name="request">Update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated food.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">Food not found.</response>
    /// <response code="409">Name conflict detected.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedFoodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdatedFoodResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateFoodRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateFoodCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedFoodResponse> response = await mediator.Send(command, cancellationToken);

        return response.Match(
            updated => Ok(updated),
            errors => errors.MapToProblem(HttpContext));
    }
}
