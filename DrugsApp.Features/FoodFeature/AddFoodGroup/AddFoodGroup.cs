using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Features.FoodFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.FoodFeature.AddFoodGroup;



/// <summary>
/// Handles creation of food groups with uniqueness and membership validation.
/// </summary>
public sealed class AddFoodGroupHandler : IRequestHandler<AddFoodGroupCommand, ErrorOr<CreatedFoodGroupResponse>>
{
    private readonly IFoodGroupRepository _foodGroupRepository;
    private readonly IFoodRepository _foodRepository;

    public AddFoodGroupHandler(
        IFoodGroupRepository foodGroupRepository,
        IFoodRepository foodRepository)
    {
        _foodGroupRepository = foodGroupRepository;
        _foodRepository = foodRepository;
    }

    public async Task<ErrorOr<CreatedFoodGroupResponse>> Handle(
        AddFoodGroupCommand command,
        CancellationToken cancellationToken)
    {
        FoodGroup? existingGroup = await _foodGroupRepository.GetByNameAsync(
            command.Request.Name,
            cancellationToken);
        if (existingGroup is not null)
        {
            return Error.Conflict(
                description: "Food group name already exists.",
                metadata: new Dictionary<string, object>
                {
                    ["existingResourceId"] = existingGroup.Id.ToString()
                });
        }

        List<Food> foods = await _foodRepository.GetByIdsAsync(
            command.Request.Members,
            cancellationToken);
        if (foods.Count != command.Request.Members.Count)
        {
            IEnumerable<Guid> missingIds = command.Request.Members.Except(foods.Select(food => food.Id));
            return Error.Validation(
                description: "One or more foods were not found.",
                metadata: new Dictionary<string, object>
                {
                    ["missingResourceIds"] = string.Join(",", missingIds)
                });
        }

        bool membershipExists = await _foodGroupRepository.ExistsWithMembersAsync(
            command.Request.Members,
            cancellationToken);
        if (membershipExists)
        {
            return Error.Conflict(
                description: "A food group with the same membership already exists.",
                metadata: new Dictionary<string, object>
                {
                    ["error"] = "Duplicate membership set."
                });
        }

        ErrorOr<FoodGroup> creationResult = FoodGroup.Create(
            command.Request.Name,
            foods);
        if (creationResult.IsError)
        {
            return creationResult.Errors;
        }

        FoodGroup foodGroup = creationResult.Value;

        await _foodGroupRepository.AddAsync(foodGroup, cancellationToken);

        return foodGroup.MapToCreatedResponse();
    }
}
