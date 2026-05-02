using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.FoodFeature.UpdateFoodGroup;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DrugsApp.Features.FoodFeature.Shared;

namespace DrugsApp.Features.FoodFeature.AddFoodGroup;

/// <summary>
/// Request model for creating a food group.
/// </summary>
public sealed record AddFoodGroupRequest(
    string Name,
    IReadOnlyCollection<Guid> Members);

/// <summary>
/// Command for creating a food group.
/// </summary>
public sealed record AddFoodGroupCommand(
    AddFoodGroupRequest Request) : IRequest<ErrorOr<CreatedFoodGroupResponse>>;

/// <summary>
/// Response containing created food group details.
/// </summary>
public sealed record CreatedFoodGroupResponse(
    FoodGroupDto FoodGroup);

/// <summary>
/// Controller for creating food groups.
/// </summary>
[ApiController]
[Tags("Food Groups")]
[Route("api/food-groups")]
public sealed class AddFoodGroupController(
    IMediator mediator,
    IValidator<AddFoodGroupRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "AddFoodGroup";

    /// <summary>
    /// Creates a new food group with provided members.
    /// </summary>
    /// <param name="request">Food group creation payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Returns the created food group.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="409">Name or membership conflict detected.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreatedFoodGroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedFoodGroupResponse>> Create(
        [FromBody] AddFoodGroupRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddFoodGroupCommand command = request.MapToCommand();
        ErrorOr<CreatedFoodGroupResponse> response = await mediator.Send(command, cancellationToken);

        return response.Match(
            created => CreatedAtRoute(
                routeName: UpdateFoodGroupController.RouteName,
                routeValues: new { id = created.FoodGroup.Id },
                value: created),
            errors => errors.MapToProblem(HttpContext));
    }
}
