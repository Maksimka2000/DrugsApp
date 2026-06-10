namespace DrugsApp.Features.DrugProblemsFeature.SearchDrugProblemTypes;

/// <summary>
/// Mapping helpers for searching drug problem types.
/// </summary>
public static class SearchDrugProblemTypesMapping
{
    public static SearchDrugProblemTypesQuery MapToQuery(this SearchDrugProblemTypesRequest request) =>
        new(request.ProblemTypeNameQuery, request.Skip, request.Take);
}
