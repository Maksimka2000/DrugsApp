namespace DrugsApp.Features.VetDrugFeature.SearchVetDrugs;

/// <summary>
/// Mapping extensions for SearchVetDrugs use case.
/// </summary>
public static class SearchVetDrugsMapping
{
    /// <summary>
    /// Maps query parameters to SearchVetDrugsQuery.
    /// </summary>
    public static SearchVetDrugsQuery MapToQuery(
        string? name,
        string order,
        int skip,
        int take)
    {
        return new SearchVetDrugsQuery(name, order, skip, take);
    }
}

