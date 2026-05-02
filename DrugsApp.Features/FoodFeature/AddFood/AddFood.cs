using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Features.FoodFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.FoodFeature.AddFood;

/// <summary>
/// Command for creating a new food.
/// </summary>
public sealed record AddFoodCommand(
    AddFoodRequest Request) : IRequest<ErrorOr<CreatedFoodResponse>>;

/// <summary>
/// Response containing created food details.
/// </summary>
public sealed record CreatedFoodResponse(
    FoodDto Food);

/// <summary>
/// Handles creation of foods with uniqueness checks.
/// </summary>
public sealed class AddFoodHandler : IRequestHandler<AddFoodCommand, ErrorOr<CreatedFoodResponse>>
{
    private readonly IFoodRepository _foodRepository;

    public AddFoodHandler(IFoodRepository foodRepository)
    {
        _foodRepository = foodRepository;
    }

    public async Task<ErrorOr<CreatedFoodResponse>> Handle(
        AddFoodCommand command,
        CancellationToken cancellationToken)
    {
        Food? existing = await _foodRepository.GetByNameAsync(
            command.Request.Name,
            cancellationToken);
        if (existing is not null)
        {
            return Error.Conflict(
                description: "Food with the same name already exists.",
                metadata: new Dictionary<string, object>
                {
                    ["existingResourceId"] = existing.Id.ToString()
                });
        }

        ErrorOr<Food> creationResult = Food.Create(
            command.Request.Name,
            command.Request.Aliases);
        if (creationResult.IsError)
        {
            return creationResult.Errors;
        }

        Food food = creationResult.Value;

        await _foodRepository.AddAsync(food, cancellationToken);

        return food.MapToCreatedResponse();
    }
}
