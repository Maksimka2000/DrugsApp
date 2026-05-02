using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DrugsApp.Features.FoodFeature.Shared;
using FluentValidation;
using FluentValidation.Results;

namespace DrugsApp.Features.FoodFeature.RemoveFoodGroup;

/// <summary>
/// Request for removing a food group.
/// </summary>
/// <param name="Id"></param>
public sealed record RemoveFoodGroupRequest(
    Guid Id);

/// <summary>
/// Command for removing a food group.
/// </summary>
public sealed record RemoveFoodGroupCommand(
    Guid Id) : IRequest<ErrorOr<DeletedFoodGroupResponse>>;

/// <summary>
/// Response containing deleted food group details.
/// </summary>
public sealed record DeletedFoodGroupResponse(
    FoodGroupDto FoodGroup);


/// <summary>
/// Controller for removing food groups.
/// </summary>
[ApiController]
[Tags("Food Groups")]
[Route("api/food-groups")]
public sealed class RemoveFoodGroupController(
    IMediator mediator, 
    IValidator<RemoveFoodGroupRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "RemoveFoodGroup";

    /// <summary>
    /// Deletes a food group when it is not referenced.
    /// </summary>
    /// <param name="id">Food group identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns deleted food group details.</response>
    /// <response code="404">Food group not found.</response>
    /// <response code="409">Food group is referenced and cannot be removed.</response>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(DeletedFoodGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DeletedFoodGroupResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {

        RemoveFoodGroupRequest request = new(id);
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }


        RemoveFoodGroupCommand command = new(id);
        ErrorOr<DeletedFoodGroupResponse> response = await mediator.Send(command, cancellationToken);

        return response.Match(
            deleted => Ok(deleted),
            errors => errors.MapToProblem(HttpContext));
    }
}
