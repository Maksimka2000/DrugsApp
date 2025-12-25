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
internal sealed class SearchActiveSubstancesHandler
    : IRequestHandler<SearchActiveSubstancesQuery, ErrorOr<SearchActiveSubstancesResponse>>
{
    private readonly IActiveSubstanceRepository _activeSubstanceRepository;

    public SearchActiveSubstancesHandler(IActiveSubstanceRepository activeSubstanceRepository)
    {
        _activeSubstanceRepository = activeSubstanceRepository;
    }

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
        string? normalizedNameFilter = string.IsNullOrWhiteSpace(query.Name)
            ? null
            : query.Name.Trim();

        List<ActiveSubstance> pagePlusOne = await _activeSubstanceRepository.SearchAsync(
            normalizedNameFilter,
            query.Skip,
            query.Take + 1,
            cancellationToken);

        bool hasMore = pagePlusOne.Count > query.Take;

        List<ActiveSubstanceDto> items = pagePlusOne
            .Take(query.Take)
            .MapToDtoList();

        int? totalCount = null;
        if (query.Skip == 0)
        {
            totalCount = await _activeSubstanceRepository.CountAsync(
                normalizedNameFilter,
                cancellationToken);
        }

        return items.MapToResponse(totalCount, hasMore);
    }
}


