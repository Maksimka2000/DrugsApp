using DrugsApp.Features.FoodFeature.Shared;

namespace DrugsApp.Features.FoodFeature.SearchFoods;

/// <summary>
/// Mapping helpers for SearchFoods use case.
/// </summary>
public static class SearchFoodsMapping
{
    /// <summary>
    /// Maps request DTO to query with normalized filters.
    /// </summary>
    public static SearchFoodsQuery MapToQuery(this SearchFoodsRequest request) => new (
            SearchText: request.FoodName,
            Skip: request.Skip,
            Take: request.Take
    );
}
