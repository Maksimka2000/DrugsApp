using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularDrugsFeature.ListRegularDrugHistory;

/// <summary>
/// Handles listing regular drug history rows for the current user.
/// </summary>
internal sealed class ListRegularDrugHistoryHandler(
    IClientRegularDrugRepository _clientRegularDrugRepository)
    : IRequestHandler<ListRegularDrugHistoryQuery, ErrorOr<ListRegularDrugHistoryResponse>>
{
    public async Task<ErrorOr<ListRegularDrugHistoryResponse>> Handle(
        ListRegularDrugHistoryQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedDrugNameQuery = string.IsNullOrWhiteSpace(query.DrugNameQuery)
            ? null
            : query.DrugNameQuery.Trim();

        List<Domain.RegularDrugsFeature.Entities.ClientRegularDrug> page =
            await _clientRegularDrugRepository.SearchHistoryByUserIdAsync(
                query.UserId,
                normalizedDrugNameQuery,
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
            totalCount = await _clientRegularDrugRepository.CountHistoryByUserIdAsync(
                query.UserId,
                normalizedDrugNameQuery,
                query.StartDate,
                query.EndDate,
                cancellationToken);
        }

        return page.MapToResponse(totalCount, hasMore);
    }
}
