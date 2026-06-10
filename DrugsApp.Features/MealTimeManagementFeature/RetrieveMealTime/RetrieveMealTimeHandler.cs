using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using DrugsApp.Domain.MealTimeManagementFeature.Repositories;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.MealTimeManagementFeature.RetrieveMealTime;

/// <summary>
/// Handles retrieval of meal times for the current user.
/// </summary>
internal sealed class RetrieveMealTimeHandler(IClientMealTimeRepository _mealTimeRepository)
    : IRequestHandler<RetrieveMealTimeQuery, ErrorOr<MealTimeDto>>
{
    public async Task<ErrorOr<MealTimeDto>> Handle(
        RetrieveMealTimeQuery query,
        CancellationToken cancellationToken)
    {
        ClientMealTime? entity = await _mealTimeRepository.GetByIdForUserAsync(
            query.UserId,
            query.MealTimeId,
            tracking: false,
            cancellationToken);

        if (entity is null)
        {
            return Error.NotFound(
                code: "MealTime.NotFound",
                description: "Meal time not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientMealTime",
                    ["resourceId"] = query.MealTimeId.ToString()
                });
        }

        return entity.MapToDto();
    }
}
