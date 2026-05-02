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

namespace DrugsApp.Features.FoodFeature.UpdateFoodGroup;



/// <summary>
/// Request model for updating a food group.
/// </summary>
public sealed record UpdateFoodGroupRequest(
    string? Name,
    IReadOnlyCollection<Guid> Members);

/// <summary>
/// Command for updating a food group aggregate.
/// </summary>
public sealed record UpdateFoodGroupCommand(
    Guid Id,
    UpdateFoodGroupRequest Request) : IRequest<ErrorOr<UpdatedFoodGroupResponse>>;

/// <summary>
/// Response containing updated food group details.
/// </summary>
public sealed record UpdatedFoodGroupResponse(
    FoodGroupDto FoodGroup);



/// <summary>
/// Controller for updating food groups.
/// </summary>
[ApiController]
[Tags("Food Groups")]
[Route("api/food-groups")]
public sealed class UpdateFoodGroupController(
    IMediator mediator,
    IValidator<UpdateFoodGroupRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "UpdateFoodGroup";

    /// <summary>
    /// Updates a food group name and membership set.
    /// </summary>
    /// <param name="id">Food group identifier.</param>
    /// <param name="request">Update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated food group.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">Food group not found.</response>
    /// <response code="409">Name conflict detected.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedFoodGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdatedFoodGroupResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateFoodGroupRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateFoodGroupCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedFoodGroupResponse> response = await mediator.Send(command, cancellationToken);

        return response.Match(
            updated => Ok(updated),
            errors => errors.MapToProblem(HttpContext));
    }
}
