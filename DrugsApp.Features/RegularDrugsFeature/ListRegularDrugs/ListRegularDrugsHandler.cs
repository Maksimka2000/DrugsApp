using System;
using System.Linq;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularDrugsFeature.ListRegularDrugs;

/// <summary>
/// Handles listing live regular drug schedules for the current user.
/// </summary>
internal sealed class ListRegularDrugsHandler(
    IClientRegularDrugRepository _clientRegularDrugRepository)
    : IRequestHandler<ListRegularDrugsQuery, ErrorOr<ListRegularDrugsResponse>>
{
    public async Task<ErrorOr<ListRegularDrugsResponse>> Handle(
        ListRegularDrugsQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedDrugNameQuery = string.IsNullOrWhiteSpace(query.DrugNameQuery)
            ? null
            : query.DrugNameQuery.Trim();

        List<Domain.RegularDrugsFeature.Entities.ClientRegularDrug> page =
            await _clientRegularDrugRepository.SearchLiveByUserIdAsync(
                query.UserId,
                normalizedDrugNameQuery,
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
            totalCount = await _clientRegularDrugRepository.CountLiveByUserIdAsync(
                query.UserId,
                normalizedDrugNameQuery,
                query.StartDate,
                cancellationToken);
        }

        return page.MapToResponse(totalCount, hasMore);
    }
}
