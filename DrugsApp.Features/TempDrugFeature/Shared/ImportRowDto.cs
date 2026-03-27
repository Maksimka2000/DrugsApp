namespace DrugsApp.Features.TempDrugFeature.Shared;

/// <summary>
/// Parsed row payload used by internal chunk commands.
/// </summary>
public sealed record ImportRowDto(
    string? ProductIdentifier,
    string? CommercialName,
    string? ActiveSubstance,
    string? TypeOfProduct,
    string? RouteOfAdministration,
    string? PharmaceuticalFormulation,
    string? AtcCode,
    string? Leaflet,
    string? EducationalMaterials,
    string? Dosage);

