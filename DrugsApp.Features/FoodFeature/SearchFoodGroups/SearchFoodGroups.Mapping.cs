namespace DrugsApp.Features.FoodFeature.SearchFoodGroups;

/// <summary>
/// Mapping helpers for SearchFoodGroups use case.
/// </summary>
public static class SearchFoodGroupsMapping
{
    /// <summary>
    /// Maps request to query with normalized filters.
    /// </summary>
    public static SearchFoodGroupsQuery MapToQuery(this SearchFoodGroupsRequest request) => new (
            FoodNameQuery: request.FoodNameQuery,
            Skip: request.Skip,
            Take: request.Take
    );
}
