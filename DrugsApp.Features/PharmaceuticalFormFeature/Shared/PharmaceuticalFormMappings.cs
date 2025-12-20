using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;

namespace DrugsApp.Features.PharmaceuticalFormFeature.Shared;

/// <summary>
/// Mapping extensions for PharmaceuticalForm entity to DTO transformations.
/// </summary>
public static class PharmaceuticalFormMappings
{
    /// <summary>
    /// Maps a PharmaceuticalForm entity to its DTO representation.
    /// </summary>
    public static PharmaceuticalFormDto MapToDto(this PharmaceuticalForm pharmaceuticalForm) => new(
        Id: pharmaceuticalForm.Id,
        FormulationName: pharmaceuticalForm.FormulationName);

    /// <summary>
    /// Maps a collection of PharmaceuticalForm entities to a list of DTOs.
    /// </summary>
    public static List<PharmaceuticalFormDto> MapToDtoList(
        this IEnumerable<PharmaceuticalForm> pharmaceuticalForms)
        => pharmaceuticalForms.Select(pharmaceuticalForm => pharmaceuticalForm.MapToDto()).ToList();
}


