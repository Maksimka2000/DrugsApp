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
        this SearchVetDrugsRequest request) => new(
            Name: request.Name, 
            Order: request.Order, 
            Skip: request.Skip, 
            Take: request.Take);
}

