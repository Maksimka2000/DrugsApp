using System.Collections.Generic;
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
public sealed class RemoveFoodGroupHandler : IRequestHandler<RemoveFoodGroupCommand, ErrorOr<DeletedFoodGroupResponse>>
{
    private readonly IFoodGroupRepository _foodGroupRepository;

    public RemoveFoodGroupHandler(IFoodGroupRepository foodGroupRepository)
    {
        _foodGroupRepository = foodGroupRepository;
    }

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

        // bool isReferenced = await _foodGroupRepository.IsReferencedByInteractionsAsync(foodGroup.Id, cancellationToken);
        // if (isReferenced)
        // {
        //     return Error.Conflict(
        //         description: "Food group is referenced and cannot be removed.",
        //         metadata: new Dictionary<string, object>
        //         {
        //             ["resourceId"] = foodGroup.Id.ToString()
        //         });
        // }

        _foodGroupRepository.Remove(foodGroup);

        return foodGroup.MapToDeletedResponse();
    }
}
