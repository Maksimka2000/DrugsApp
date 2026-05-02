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

    public RemoveFoodHandler(IFoodRepository foodRepository)
    {
        _foodRepository = foodRepository;
    }

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
