using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.VetDrugFeature.Entities;

namespace DrugsApp.Features.VetDrugFeature.Shared;

/// <summary>
/// Mapping extensions for VetDrug entity to DTO transformations.
/// </summary>
public static class VetDrugMappings
{
    /// <summary>
    /// Maps a VetDrug entity to its DTO representation.
    /// </summary>
    public static VetDrugDto MapToDto(this VetDrug vetDrug) => new (
            Id: vetDrug.Id,
            ProductIdentifier: vetDrug.ProductIdentifier,
            CommercialName: vetDrug.CommercialName,
            TypeOfProduct: vetDrug.TypeOfProduct);

    /// <summary>
    /// Maps a collection of VetDrug entities to a list of DTOs.
    /// </summary>
    public static List<VetDrugDto> MapToDtoList(this IEnumerable<VetDrug> vetDrugs) => vetDrugs.Select(vetDrug => vetDrug.MapToDto()).ToList();
   
}

