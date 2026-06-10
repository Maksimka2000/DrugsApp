using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplements;

/// <summary>
/// Handles listing live regular supplement schedules for the current user.
/// </summary>
internal sealed class ListRegularSupplementsHandler(
    IClientRegularSupplementRepository _clientRegularSupplementRepository)
    : IRequestHandler<ListRegularSupplementsQuery, ErrorOr<ListRegularSupplementsResponse>>
{
    public async Task<ErrorOr<ListRegularSupplementsResponse>> Handle(
        ListRegularSupplementsQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedNameQuery = string.IsNullOrWhiteSpace(query.NameQuery)
            ? null
            : query.NameQuery.Trim();

        List<Domain.RegularSupplementsFeature.Entities.ClientRegularSupplement> page =
            await _clientRegularSupplementRepository.SearchLiveByUserIdAsync(
                query.UserId,
                normalizedNameQuery,
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
            totalCount = await _clientRegularSupplementRepository.CountLiveByUserIdAsync(
                query.UserId,
                normalizedNameQuery,
                query.StartDate,
                cancellationToken);
        }

        return page.MapToResponse(totalCount, hasMore);
    }
}
