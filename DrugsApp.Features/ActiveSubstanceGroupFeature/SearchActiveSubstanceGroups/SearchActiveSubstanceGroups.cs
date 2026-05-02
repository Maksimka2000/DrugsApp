using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Features.ActiveSubstanceGroupFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.SearchActiveSubstanceGroups;

/// <summary>
/// Handles searching active substance groups with optional member-name filtering.
/// </summary>
internal sealed class SearchActiveSubstanceGroupsHandler(IActiveSubstanceGroupRepository _activeSubstanceGroupRepository)
    : IRequestHandler<SearchActiveSubstanceGroupsQuery, ErrorOr<SearchActiveSubstanceGroupsResponse>>
{
    /// <summary>
    /// Searches active substance groups and returns paged results.
    /// </summary>
    /// <param name="query">Search query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<ErrorOr<SearchActiveSubstanceGroupsResponse>> Handle(
        SearchActiveSubstanceGroupsQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedActiveSubstanceName = NormalizeActiveSubstanceName(query.ActiveSubstanceName);
        int takePlusOne = query.Take + 1;
        int? totalCount = null;

        List<ActiveSubstanceGroup> groups = await _activeSubstanceGroupRepository.SearchAsync(
            normalizedActiveSubstanceName,
            query.Skip,
            takePlusOne,
            cancellationToken);

        bool hasMore = groups.Count > query.Take;
        List<ActiveSubstanceGroup> page = hasMore
            ? groups.Take(query.Take).ToList()
            : groups;

        List<ActiveSubstanceGroupDto> items = page.MapToDtoList();

        if (query.Skip == 0)
        {
            totalCount = await _activeSubstanceGroupRepository.CountAsync(
                normalizedActiveSubstanceName,
                cancellationToken);
        }

        return items.MapToResponse(totalCount, hasMore);
    }

    private static string? NormalizeActiveSubstanceName(string? activeSubstanceName)
    {
        if (string.IsNullOrWhiteSpace(activeSubstanceName))
        {
            return null;
        }

        return activeSubstanceName.Trim();
    }
}
