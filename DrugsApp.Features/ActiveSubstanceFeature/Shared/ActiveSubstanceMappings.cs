using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;

namespace DrugsApp.Features.ActiveSubstanceFeature.Shared;

/// <summary>
/// Mapping extensions for ActiveSubstance aggregate to DTO transformations.
/// </summary>
public static class ActiveSubstanceMappings
{
    /// <summary>
    /// Maps an ActiveSubstance entity to its DTO representation.
    /// </summary>
    public static ActiveSubstanceDto MapToDto(this ActiveSubstance activeSubstance) => new(
        Id: activeSubstance.Id,
        ActiveSubstanceName: activeSubstance.ActiveSubstanceName,
        OralMedicationIntakeGuidelines: activeSubstance.OralMedicationIntakeGuidelines?.MapToDto());

    /// <summary>
    /// Maps a collection of ActiveSubstance entities to a list of DTOs.
    /// </summary>
    public static List<ActiveSubstanceDto> MapToDtoList(this IEnumerable<ActiveSubstance> activeSubstances)
        => activeSubstances.Select(activeSubstance => activeSubstance.MapToDto()).ToList();

    /// <summary>
    /// Maps OralMedicationIntakeGuidelines entity to its DTO representation.
    /// </summary>
    public static OralMedicationIntakeGuidelinesDto MapToDto(this OralMedicationIntakeGuidelines guidelines) => new(
        FoodIntakeType: guidelines.FoodIntakeType,
        WaitBeforeMinutes: guidelines.WaitBeforeMinutes,
        WaitAfterMinutes: guidelines.WaitAfterMinutes,
        MealNotes: guidelines.MealNotes,
        AvoidFoodDrug: guidelines.AvoidFoodDrug,
        AvoidFoodTherapy: guidelines.AvoidFoodTherapy,
        LiquidNotes: guidelines.LiquidNotes,
        LiquidInstructions: guidelines.LiquidInstructions,
        RecommendedMorning: guidelines.RecommendedMorning,
        RecommendedEvening: guidelines.RecommendedEvening);
}


