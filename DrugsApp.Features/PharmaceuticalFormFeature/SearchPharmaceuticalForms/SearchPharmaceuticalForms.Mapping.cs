using DrugsApp.Features.PharmaceuticalFormFeature.Shared;

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
        FormulationName: request.FormulationName,
        Skip: request.Skip,
        Take: request.Take);

    /// <summary>
    /// Maps the response
    /// </summary>
    public static SearchPharmaceuticalFormsResponse MapToResponse(
        this List<PharmaceuticalFormDto> items,
        int? totalCount,
        bool hasMore) => new(
        Items: items,
        TotalCount: totalCount,
        HasMore: hasMore);
}


