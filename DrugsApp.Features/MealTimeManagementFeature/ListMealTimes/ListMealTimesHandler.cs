using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using DrugsApp.Domain.MealTimeManagementFeature.Repositories;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.MealTimeManagementFeature.ListMealTimes;

/// <summary>
/// Handles listing meal times for the current user.
/// </summary>
internal sealed class ListMealTimesHandler(IClientMealTimeRepository _mealTimeRepository)
    : IRequestHandler<ListMealTimesQuery, ErrorOr<ListMealTimesResponse>>
{
    public async Task<ErrorOr<ListMealTimesResponse>> Handle(
        ListMealTimesQuery query,
        CancellationToken cancellationToken)
    {
        System.Collections.Generic.List<ClientMealTime> rows = await _mealTimeRepository.ListByUserAsync(
            query.UserId,
            cancellationToken);

        return rows.MapToResponse();
    }
}
