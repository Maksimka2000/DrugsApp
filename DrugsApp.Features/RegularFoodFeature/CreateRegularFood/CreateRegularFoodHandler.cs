using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Features.RegularFoodFeature.Shared;
using DrugsApp.Features.Common.Limits;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularFoodFeature.CreateRegularFood;

/// <summary>
/// Handles creation of regular food schedules.
/// </summary>
internal sealed class CreateRegularFoodHandler(
    IFoodRepository _foodRepository,
    IClientRegularFoodRepository _clientRegularFoodRepository)
    : IRequestHandler<CreateRegularFoodCommand, ErrorOr<CreateRegularFoodResponse>>
{
    public async Task<ErrorOr<CreateRegularFoodResponse>> Handle(
        CreateRegularFoodCommand command,
        CancellationToken cancellationToken)
    {
        int existingCount = await _clientRegularFoodRepository.CountLiveByUserIdAsync(
            command.UserId,
            foodNameQuery: null,
            startDate: null,
            cancellationToken);
        ErrorOr<Success> limitCheck = UserRecordLimits.EnsureBelowLimit(
            existingCount,
            UserRecordLimits.MAX_REGULAR_FOODS,
            "RegularFood");
        if (limitCheck.IsError)
        {
            return limitCheck.Errors;
        }

        Food? food = await _foodRepository.GetByIdAsync(command.Request.FoodId, cancellationToken);
        if (food is null)
        {
            return Error.NotFound(
                description: "Food not found.",
                metadata: new()
                {
                    ["resourceType"] = "Food",
                    ["resourceId"] = command.Request.FoodId.ToString(),
                });
        }

        bool hasDuplicate = await _clientRegularFoodRepository.ExistsLiveForUserWithFoodIdAsync(
            command.UserId,
            command.Request.FoodId,
            excludeId: null,
            cancellationToken);
        if (hasDuplicate)
        {
            return Error.Conflict(
                description: "A live regular food schedule already exists for this food.",
                metadata: new()
                {
                    ["field"] = "foodId",
                    ["conflictingResourceId"] = command.Request.FoodId.ToString(),
                });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        ErrorOr<ClientRegularFood> created = ClientRegularFood.Create(
            command.UserId,
            command.Request.FoodId,
            command.Request.StartDate,
            command.Request.EndDate,
            command.Request.IntakeTimes,
            today);
        if (created.IsError)
        {
            return created.Errors;
        }

        created.Value.AttachFood(food);

        await _clientRegularFoodRepository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToListItemDto().MapToResponse();
    }
}
