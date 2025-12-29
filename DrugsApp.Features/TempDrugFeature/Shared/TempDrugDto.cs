using System;
using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Features.TempDrugFeature.Shared;

/// <summary>
/// Data transfer object representing an imported temporary drug record.
/// </summary>
public sealed record TempDrugDto(
    Guid Id,
    string ProductIdentifier,
    string? CommercialName,
    string? ActiveSubstance,
    string? TypeOfProduct,
    string? RouteOfAdministration,
    string? PharmaceuticalFormulation,
    string? ATCCode,
    string? Leaflet,
    string? EducationalMaterials,
    string? Dosage,
    Guid? LinkedDrugFormId,
    Guid? LinkedDrugVetId,
    Guid LinkedImportId,
    TempDrugStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);


