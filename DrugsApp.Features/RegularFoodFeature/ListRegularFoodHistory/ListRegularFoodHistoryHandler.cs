using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularFoodFeature.ListRegularFoodHistory;

/// <summary>
/// Handles listing regular food history rows for the current user.
/// </summary>
internal sealed class ListRegularFoodHistoryHandler(
    IClientRegularFoodRepository _clientRegularFoodRepository)
    : IRequestHandler<ListRegularFoodHistoryQuery, ErrorOr<ListRegularFoodHistoryResponse>>
{
    public async Task<ErrorOr<ListRegularFoodHistoryResponse>> Handle(
        ListRegularFoodHistoryQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedFoodNameQuery = string.IsNullOrWhiteSpace(query.FoodNameQuery)
            ? null
            : query.FoodNameQuery.Trim();

        List<Domain.RegularFoodFeature.Entities.ClientRegularFood> page =
            await _clientRegularFoodRepository.SearchHistoryByUserIdAsync(
                query.UserId,
                normalizedFoodNameQuery,
                query.StartDate,
                query.EndDate,
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
            totalCount = await _clientRegularFoodRepository.CountHistoryByUserIdAsync(
                query.UserId,
                normalizedFoodNameQuery,
                query.StartDate,
                query.EndDate,
                cancellationToken);
        }

        return page.MapToResponse(totalCount, hasMore);
    }
}
