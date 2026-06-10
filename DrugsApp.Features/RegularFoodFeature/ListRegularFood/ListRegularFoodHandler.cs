using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularFoodFeature.ListRegularFood;

/// <summary>
/// Handles listing live regular food schedules for the current user.
/// </summary>
internal sealed class ListRegularFoodHandler(
    IClientRegularFoodRepository _clientRegularFoodRepository)
    : IRequestHandler<ListRegularFoodQuery, ErrorOr<ListRegularFoodResponse>>
{
    public async Task<ErrorOr<ListRegularFoodResponse>> Handle(
        ListRegularFoodQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedFoodNameQuery = string.IsNullOrWhiteSpace(query.FoodNameQuery)
            ? null
            : query.FoodNameQuery.Trim();

        List<Domain.RegularFoodFeature.Entities.ClientRegularFood> page =
            await _clientRegularFoodRepository.SearchLiveByUserIdAsync(
                query.UserId,
                normalizedFoodNameQuery,
                query.StartDate,
                query.Skip,
                query.Take + 1,
                cancellationToken);

        bool hasMore = page.Count > query.Take;
        if (hasMore)
        {
            page = page.Take(query.Take).ToList();
        }

        int? totalCount = null;
        if (query.Skip == 0)
        {
            totalCount = await _clientRegularFoodRepository.CountLiveByUserIdAsync(
                query.UserId,
                normalizedFoodNameQuery,
                query.StartDate,
                cancellationToken);
        }

        return page.MapToResponse(totalCount, hasMore);
    }
}
