namespace DrugsApp.Features.PharmaceuticalFormFeature.SearchPharmaceuticalForms;

/// <summary>
/// Mapping extensions for SearchPharmaceuticalForms use case.
/// </summary>
public static class SearchPharmaceuticalFormsMapping
{
    /// <summary>
    /// Maps query parameters to SearchPharmaceuticalFormsQuery.
    /// </summary>
    public static SearchPharmaceuticalFormsQuery MapToQuery(
        this SearchPharmaceuticalFormsRequest request) => new(
        Name: request.Name,
        Skip: request.Skip,
        Take: request.Take);
}


