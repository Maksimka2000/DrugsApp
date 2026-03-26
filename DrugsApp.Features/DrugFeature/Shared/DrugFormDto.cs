using System;
using System.Collections.Generic;

namespace DrugsApp.Features.DrugFeature.Shared;

/// <summary>
/// Data transfer object representing a drug form of a drug.
/// </summary>
public sealed record DrugFormDto(
    Guid Id,
    int ProductIdentifier,
    string? AtcCode,
    string? Dosage,
    string? Leaflet,
    string? EducationMaterials,
    bool IsOral,
    IReadOnlyCollection<LookupDto> RoutesOfAdministration,
    IReadOnlyCollection<LookupDto> PharmaceuticalForms);

