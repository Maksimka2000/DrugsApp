using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplementHistory;

/// <summary>
/// Handles listing regular supplement history rows for the current user.
/// </summary>
internal sealed class ListRegularSupplementHistoryHandler(
    IClientRegularSupplementRepository _clientRegularSupplementRepository)
    : IRequestHandler<ListRegularSupplementHistoryQuery, ErrorOr<ListRegularSupplementHistoryResponse>>
{
    public async Task<ErrorOr<ListRegularSupplementHistoryResponse>> Handle(
        ListRegularSupplementHistoryQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedNameQuery = string.IsNullOrWhiteSpace(query.NameQuery)
            ? null
            : query.NameQuery.Trim();

        List<Domain.RegularSupplementsFeature.Entities.ClientRegularSupplement> page =
            await _clientRegularSupplementRepository.SearchHistoryByUserIdAsync(
                query.UserId,
                normalizedNameQuery,
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
            totalCount = await _clientRegularSupplementRepository.CountHistoryByUserIdAsync(
                query.UserId,
                normalizedNameQuery,
                query.StartDate,
                query.EndDate,
                cancellationToken);
        }

        return page.MapToResponse(totalCount, hasMore);
    }
}
