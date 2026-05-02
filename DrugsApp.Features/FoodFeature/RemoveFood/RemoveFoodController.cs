using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DrugsApp.Features.FoodFeature.Shared;

namespace DrugsApp.Features.FoodFeature.RemoveFood;

/// <summary>
/// Request for removing a food.
/// </summary>
/// <param name="Id"></param>
public sealed record RemoveFoodRequest(
    Guid Id);

/// <summary>
/// Command for removing a food.
/// </summary>
public sealed record RemoveFoodCommand(
    Guid Id) : IRequest<ErrorOr<DeletedFoodResponse>>;

/// <summary>
/// Response containing deleted food details.
/// </summary>
public sealed record DeletedFoodResponse(
    FoodDto Food);


/// <summary>
/// Controller for removing foods.
/// </summary>
[ApiController]
[Tags("Foods")]
[Route("api/foods")]
public sealed class RemoveFoodController(IMediator mediator, IValidator<RemoveFoodRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "RemoveFood";

    /// <summary>
    /// Deletes a food when it is not referenced.
    /// </summary>
    /// <param name="id">Food identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns deleted food details.</response>
    /// <response code="404">Food not found.</response>
    /// <response code="409">Food is referenced and cannot be removed.</response>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(DeletedFoodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DeletedFoodResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {

        RemoveFoodRequest request = new(id);
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        RemoveFoodCommand command = new(id);
        ErrorOr<DeletedFoodResponse> response = await mediator.Send(command, cancellationToken);

        return response.Match(
            deleted => Ok(deleted),
            errors => errors.MapToProblem(HttpContext));
    }
}
