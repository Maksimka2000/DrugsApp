using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Features.FoodFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.FoodFeature.SearchFoods;



/// <summary>
/// Handles searching foods by name and aliases with paging.
/// </summary>
public sealed class SearchFoodsHandler(IFoodRepository _foodRepository) : IRequestHandler<SearchFoodsQuery, ErrorOr<SearchFoodsResponse>>
{
    /// <summary>
    /// Processes the given query to search for foods based on the specified parameters,
    /// retrieves the matching results from the repository, and returns the search response.
    /// </summary>
    /// <param name="query">
    /// The query containing the search text, number of items to skip, and number of items to take.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// An <c>ErrorOr&lt;SearchFoodsResponse&gt;</c> object containing the search results,
    /// total count (if applicable), and a flag indicating whether more results are available.
    /// </returns>
    public async Task<ErrorOr<SearchFoodsResponse>> Handle(
        SearchFoodsQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedSearchText = string.IsNullOrWhiteSpace(query.SearchText)
            ? null
            : query.SearchText!.Trim();

        int takePlusOne = query.Take + 1;

        List<Domain.FoodFeature.Entities.Food> foods = await _foodRepository.SearchAsync(
            normalizedSearchText,
            query.Skip,
            takePlusOne,
            cancellationToken);

        bool hasMore = foods.Count > query.Take;
        List<FoodDto> items = foods
            .Take(query.Take)
            .MapToDtoList();

        int? totalCount = null;
        if (query.Skip == 0)
        {
            totalCount = await _foodRepository.CountAsync(
                normalizedSearchText,
                cancellationToken);
        }

        return new SearchFoodsResponse(
            Items: items,
            TotalCount: totalCount,
            HasMore: hasMore);
    }
}
