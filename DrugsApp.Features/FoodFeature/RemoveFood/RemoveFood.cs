using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Features.FoodFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.FoodFeature.RemoveFood;



/// <summary>
/// Handles deletion of foods with reference checks.
/// </summary>
public sealed class RemoveFoodHandler : IRequestHandler<RemoveFoodCommand, ErrorOr<DeletedFoodResponse>>
{
    private readonly IFoodRepository _foodRepository;

    /// <summary>
    /// Handles the process of removing a food item from the system.
    /// </summary>
    public RemoveFoodHandler(IFoodRepository foodRepository)
    {
        _foodRepository = foodRepository;
    }

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
                description: "Food was not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "Food",
                    ["resourceId"] = command.Id.ToString()
                });
        }

        // bool isReferenced = await _foodRepository.IsReferencedAsync(food.Id, cancellationToken);
        // if (isReferenced)
        // {
        //     return Error.Conflict(
        //         description: "Food cannot be removed because it is referenced by other resources.",
        //         metadata: new Dictionary<string, object>
        //         {
        //             ["resourceId"] = food.Id.ToString()
        //         });
        // }

        _foodRepository.Remove(food);

        return food.MapToDeletedResponse();
    }
}
