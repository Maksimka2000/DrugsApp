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

    /// <summary>
    /// Handles the execution of the <see cref="AddFoodGroupCommand"/> to create a new food group.
    /// Performs validation to ensure the food group is unique, the referenced food items exist,
    /// and all required data is provided.
    /// </summary>
    /// <param name="foodGroupRepository"></param>
    /// <param name="foodRepository"></param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> result containing the response with the created food group
    /// if the operation is successful, or a list of errors if the operation fails.
    /// </returns>
    public AddFoodGroupHandler(
        IFoodGroupRepository foodGroupRepository,
        IFoodRepository foodRepository)
    {
        _foodGroupRepository = foodGroupRepository;
        _foodRepository = foodRepository;
    }

    /// <summary>
    /// Handles the execution of the <see cref="AddFoodGroupCommand"/> to create and manage a food group.
    /// Validates input, checks for conflicts, and ensures all referenced food items are valid.
    /// </summary>
    /// <param name="command">The command containing the details of the food group to be added, including its name and members.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> result containing the created food group response if the operation succeeds,
    /// or a list of errors describing any issues encountered during execution.
    /// </returns>
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
