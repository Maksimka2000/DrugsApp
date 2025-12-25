using DrugsApp.Domain.ActiveSubstanceFeature.Entities;

namespace DrugsApp.Features.ActiveSubstanceFeature.Shared;

/// <summary>
/// Data transfer object representing oral medication intake guidelines.
/// </summary>
public sealed record AddOralMedicationIntakeGuidelinesRequest(
    FoodIntakeType FoodIntakeType,
    int? WaitBeforeMinutes,
    int? WaitAfterMinutes,
    string? MealNotes,
    string? AvoidFoodDrug,
    string? AvoidFoodTherapy,
    string? LiquidNotes,
    bool? LiquidInstructions,
    bool? RecommendedMorning,
    bool? RecommendedEvening);


