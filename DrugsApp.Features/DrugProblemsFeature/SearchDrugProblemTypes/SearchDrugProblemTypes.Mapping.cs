namespace DrugsApp.Features.DrugProblemsFeature.SearchDrugProblemTypes;

/// <summary>
/// Mapping helpers for searching drug problem types.
/// </summary>
public static class SearchDrugProblemTypesMapping
{
    /// <summary>
    /// Builds a query to search drug problem types with optional name filter.
    /// </summary>
    /// <param name="request">Search request containing name query and paging options.</param>
    /// <returns>Query ready for handler execution.</returns>
    public static SearchDrugProblemTypesQuery MapToQuery(this SearchDrugProblemTypesRequest request) =>
        new(request.ProblemTypeNameQuery, request.Skip, request.Take);
}
