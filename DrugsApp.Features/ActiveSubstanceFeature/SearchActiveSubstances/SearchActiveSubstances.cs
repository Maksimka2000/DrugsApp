using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ActiveSubstanceFeature.SearchActiveSubstances;

/// <summary>
/// Handles searching active substances by name with pagination metadata.
/// </summary>
internal sealed class SearchActiveSubstancesHandler(IActiveSubstanceRepository _activeSubstanceRepository)
    : IRequestHandler<SearchActiveSubstancesQuery, ErrorOr<SearchActiveSubstancesResponse>>
{

    /// <summary>
    /// Searches active substances with paging and optional total count for the first page.
    /// </summary>
    /// <param name="query">Search parameters including name filter and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search response containing items and paging metadata.</returns>
    public async Task<ErrorOr<SearchActiveSubstancesResponse>> Handle(
        SearchActiveSubstancesQuery query,
        CancellationToken cancellationToken)
    {
        // initialize normalized name filter
        string? normalizedActiveSubstanceName = NormalizeActiveSubstanceName(query.ActiveSubstanceName);
        // nomalize filter
        // determine sort order
        // get sort field
        // initialize take plus one
        int takePlusOne = query.Take + 1;
        // initialize total count
        int? totalCount = null;

        // search active substances
        List<ActiveSubstance> activeSubstances = await _activeSubstanceRepository.SearchAsync(
            normalizedActiveSubstanceName,
            query.Alcohol,
            query.Skip,
            takePlusOne,
            cancellationToken);

        // determine if there are more items
        bool hasMore = activeSubstances.Count > query.Take;

        // get page
        List<ActiveSubstance> page = hasMore
            ? activeSubstances.Take(query.Take).ToList()
            : activeSubstances;

        // map to DTOs
        List<ActiveSubstanceDto> items = page.MapToDtoList();

        // count total items
        if (query.Skip == 0)
        {
            totalCount = await _activeSubstanceRepository.CountAsync(
                normalizedActiveSubstanceName,
                query.Alcohol,
                cancellationToken);
        }

        return items.MapToResponse(totalCount, hasMore);
    }


    private static string? NormalizeActiveSubstanceName(string? activeSubstanceName){
        if (string.IsNullOrWhiteSpace(activeSubstanceName))
        {
            return null;
        }

        return activeSubstanceName.Trim();
    }
}



