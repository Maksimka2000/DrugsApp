using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Features.TempDrugFeature.Shared;

/// <summary>
/// Mapping extensions for TempDrug entity to DTO transformations.
/// </summary>
public static class TempDrugMappings
{
    /// <summary>
    /// Maps a TempDrug entity to its DTO representation.
    /// </summary>
    public static TempDrugDto MapToDto(this TempDrug tempDrug) => new(
        Id: tempDrug.Id,
        ProductIdentifier: tempDrug.ProductIdentifier,
        CommercialName: tempDrug.CommercialName,
        ActiveSubstance: tempDrug.ActiveSubstance,
        TypeOfProduct: tempDrug.TypeOfProduct,
        RouteOfAdministration: tempDrug.RouteOfAdministration,
        PharmaceuticalFormulation: tempDrug.PharmaceuticalFormulation,
        ATCCode: tempDrug.ATCCode,
        Leaflet: tempDrug.Leaflet,
        EducationalMaterials: tempDrug.EducationalMaterials,
        Dosage: tempDrug.Dosage,
        Note: tempDrug.Note,
        LinkedDrugFormId: tempDrug.LinkedDrugFormId,
        LinkedDrugVetId: tempDrug.LinkedDrugVetId,
        LinkedImportId: tempDrug.LinkedImportId,
        Status: tempDrug.Status,
        CreatedAtUtc: tempDrug.CreatedAtUtc,
        UpdatedAtUtc: tempDrug.UpdatedAtUtc);

    /// <summary>
    /// Maps a collection of TempDrug entities to a list of DTOs.
    /// </summary>
    public static List<TempDrugDto> MapToDtoList(this IEnumerable<TempDrug> tempDrugs)
        => tempDrugs.Select(tempDrug => tempDrug.MapToDto()).ToList();
}


