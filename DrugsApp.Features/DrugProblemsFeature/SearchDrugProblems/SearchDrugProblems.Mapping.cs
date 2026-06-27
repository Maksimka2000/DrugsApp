using System;

namespace DrugsApp.Features.DrugProblemsFeature.SearchDrugProblems;

/// <summary>
/// Mapping helpers for search drug problems flow.
/// </summary>
public static class SearchDrugProblemsMapping
{
    /// <summary>
    /// Builds a search query for drug problems scoped to a user.
    /// </summary>
    /// <param name="request">Search request containing paging arguments.</param>
    /// <param name="userId">Identifier of the user whose problems are listed.</param>
    /// <returns>Query ready for handler.</returns>
    public static SearchDrugProblemsQuery MapToQuery(this SearchDrugProblemsRequest request, Guid userId) =>
        new(userId, request.Skip, request.Take);
}
