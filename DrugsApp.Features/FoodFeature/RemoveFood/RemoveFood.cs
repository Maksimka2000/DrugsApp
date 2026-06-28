using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Features.FoodFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.FoodFeature.RemoveFood;



/// <summary>
/// Handles deletion of foods with reference checks.
/// </summary>
public sealed class RemoveFoodHandler(IFoodRepository _foodRepository)
    : IRequestHandler<RemoveFoodCommand, ErrorOr<DeletedFoodResponse>>
{
    /// <summary>
    /// Handles the removal of a food item.
    /// </summary>
    /// <param name="command">The command containing the details of the food to be removed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing the deleted food details if successful, or an error if the operation fails.</returns>
    public async Task<ErrorOr<DeletedFoodResponse>> Handle(
        RemoveFoodCommand command,
        CancellationToken cancellationToken)
    {
        Food? food = await _foodRepository.GetByIdAsync(command.Id, cancellationToken);
        if (food is null)
        {
            return Error.NotFound(
                description: "Food not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "Food",
                    ["resourceId"] = command.Id.ToString()
                });
        }

        List<FoodGroup> foodGroups = await _foodRepository.ListGroupsByFoodIdAsync(
            food.Id,
            cancellationToken);

        if (foodGroups.Count > 0)
        {
            return Error.Conflict(
                description: "Food cannot be removed because it is a member of food groups.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = food.Id.ToString(),
                    ["groupIds"] = foodGroups.Select(group => group.Id.ToString()).ToList(),
                    ["groupNames"] = foodGroups.Select(group => group.Name).ToList()
                });
        }

        IReadOnlyCollection<Guid> interactionIds = await _foodRepository.GetInteractionIdsReferencingAsync(
            food.Id,
            cancellationToken);

        if (interactionIds.Count > 0)
        {
            return Error.Conflict(
                description: "Food cannot be removed because it participates in interactions.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = food.Id.ToString(),
                    ["interactionIds"] = interactionIds.Select(interactionId => interactionId.ToString()).ToList()
                });
        }

        _foodRepository.Remove(food);

        return food.MapToDeletedResponse();
    }
}
