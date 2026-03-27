namespace DrugsApp.Features.TempDrugFeature.Shared;

/// <summary>
/// Canonical list of importable TempDrug field names.
/// </summary>
public static class TempDrugFields
{
    /// <summary>
    /// all possible fields in the temp drug entity
    /// </summary>
    public static readonly IReadOnlyList<string> All = new List<string>
    {
        "ProductIdentifier",
        "CommercialName",
        "ActiveSubstance",
        "TypeOfProduct",
        "RouteOfAdministration",
        "PharmaceuticalFormulation",
        "ATCCode",
        "Leaflet",
        "EducationalMaterials",
        "Dosage"
    };
}

