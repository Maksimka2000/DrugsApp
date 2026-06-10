using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using DrugsApp.Domain.MealTimeManagementFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.MealTimeManagementFeature.DeleteMealTime;

/// <summary>
/// Handles deletion of meal times for the current user.
/// </summary>
internal sealed class DeleteMealTimeHandler(IClientMealTimeRepository _mealTimeRepository)
    : IRequestHandler<DeleteMealTimeCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        DeleteMealTimeCommand command,
        CancellationToken cancellationToken)
    {
        ClientMealTime? entity = await _mealTimeRepository.GetByIdForUserAsync(
            command.UserId,
            command.MealTimeId,
            tracking: true,
            cancellationToken);

        if (entity is null)
        {
            return Error.NotFound(
                code: "MealTime.NotFound",
                description: "Meal time not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientMealTime",
                    ["resourceId"] = command.MealTimeId.ToString()
                });
        }

        _mealTimeRepository.Remove(entity);

        return Result.Success;
    }
}
