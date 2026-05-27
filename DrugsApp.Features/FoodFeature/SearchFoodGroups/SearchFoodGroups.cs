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

    /// <summary>
    /// Handles the processing of the SearchFoodGroupsQuery to search for food groups based on the provided query parameters.
    /// </summary>
    /// <remarks>
    /// This handler utilizes an IFoodGroupRepository to perform primary name-based food group searches.
    /// If no results are found during the primary search and a valid query exists, it may perform a fallback member-based search.
    /// It also calculates whether additional records exist beyond the requested page and, optionally, retrieves the total count of matching records.
    /// </remarks>
    /// <param name="foodGroupRepository"></param>
    /// <returns>
    /// A Task that resolves to an ErrorOr<SearchFoodGroupsResponse/>, containing the paginated list of matching food groups (as DTOs),
    /// the total count of records when applicable, and an indicator if there are more records available.
    /// </returns>
    public SearchFoodGroupsHandler(IFoodGroupRepository foodGroupRepository) => _foodGroupRepository = foodGroupRepository;

    /// <summary>
    /// Handles the processing of the SearchFoodGroupsQuery to search for food groups based on the provided query parameters.
    /// </summary>
    /// <param name="query">
    /// Represents the search criteria, containing the food name query, the number of records to skip, and the number of records to take.
    /// </param>
    /// <param name="cancellationToken">
    /// Propagates notification that the operation should be canceled.
    /// </param>
    /// <returns>
    /// A Task that resolves to an ErrorOr<SearchFoodGroupsResponse/> which contains the list of matching food groups, total count,
    /// and a flag indicating if there are more records available.
    /// </returns>
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
