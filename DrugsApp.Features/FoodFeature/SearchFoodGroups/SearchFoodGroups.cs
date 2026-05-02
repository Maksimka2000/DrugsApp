using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Features.FoodFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.FoodFeature.SearchFoodGroups;



/// <summary>
/// Handles searching food groups with primary name search and member-name fallback.
/// </summary>
public sealed class SearchFoodGroupsHandler : IRequestHandler<SearchFoodGroupsQuery, ErrorOr<SearchFoodGroupsResponse>>
{
    private readonly IFoodGroupRepository _foodGroupRepository;

    public SearchFoodGroupsHandler(IFoodGroupRepository foodGroupRepository)
    {
        _foodGroupRepository = foodGroupRepository;
    }

    public async Task<ErrorOr<SearchFoodGroupsResponse>> Handle(
        SearchFoodGroupsQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedQuery = string.IsNullOrWhiteSpace(query.FoodNameQuery)
            ? null
            : query.FoodNameQuery!.Trim();

        int takePlusOne = query.Take + 1;

        List<FoodGroup> results = await _foodGroupRepository.SearchAsyncByGroupName(
            normalizedQuery,
            query.Skip,
            takePlusOne,
            cancellationToken);

        bool usedMemberSearch = false;
        if (normalizedQuery is not null && results.Count == 0)
        {
            results = await _foodGroupRepository.SearchAsyncByMemberFoodName(
                normalizedQuery,
                query.Skip,
                takePlusOne,
                cancellationToken);
            usedMemberSearch = true;
        }

        bool hasMore = results.Count > query.Take;
        List<FoodGroupDto> items = results
            .Take(query.Take)
            .MapToDtoList();

        int? totalCount = null;
        if (query.Skip == 0)
        {
            totalCount = usedMemberSearch
                ? await _foodGroupRepository.CountByMemberFoodNameAsync(normalizedQuery ?? string.Empty, cancellationToken)
                : await _foodGroupRepository.CountByGroupNameAsync(normalizedQuery, cancellationToken);
        }

        return new SearchFoodGroupsResponse(
            Items: items,
            TotalCount: totalCount,
            HasMore: hasMore);
    }
}
