using System;

namespace DrugsApp.Features.DrugProblemsFeature.SearchDrugProblems;

/// <summary>
/// Mapping helpers for search drug problems flow.
/// </summary>
public static class SearchDrugProblemsMapping
{
    public static SearchDrugProblemsQuery MapToQuery(this SearchDrugProblemsRequest request, Guid userId) =>
        new(userId, request.Skip, request.Take);
}
