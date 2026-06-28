using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Features.FoodFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.FoodFeature.RemoveFoodGroup;



/// <summary>
/// Handles deletion of food groups with optional reference checks.
/// </summary>
public sealed class RemoveFoodGroupHandler(IFoodGroupRepository _foodGroupRepository) : IRequestHandler<RemoveFoodGroupCommand, ErrorOr<DeletedFoodGroupResponse>>
{
    /// <summary>
    /// Handles the execution of the RemoveFoodGroupCommand to remove a specific food group.
    /// </summary>
    /// <param name="command">The command containing the ID of the food group to be removed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>Returns a result encapsulating either the details of the deleted food group or an error.</returns>
    public async Task<ErrorOr<DeletedFoodGroupResponse>> Handle(
        RemoveFoodGroupCommand command,
        CancellationToken cancellationToken)
    {
        FoodGroup? foodGroup = await _foodGroupRepository.GetByIdAsync(command.Id, cancellationToken);
        if (foodGroup is null)
        {
            return Error.NotFound(
                description: "Food group was not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "FoodGroup",
                    ["resourceId"] = command.Id.ToString()
                });
        }

        IReadOnlyCollection<Guid> interactionIds = await _foodGroupRepository.GetInteractionIdsReferencingAsync(
            foodGroup.Id,
            cancellationToken);
        if (interactionIds.Any())
        {
            return Error.Conflict(
                description: "Food group cannot be removed because it is referenced by interactions.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = foodGroup.Id.ToString(),
                    ["blockingInteractionIds"] = interactionIds
                });
        }

        _foodGroupRepository.Remove(foodGroup);

        return foodGroup.MapToDeletedResponse();
    }
}
