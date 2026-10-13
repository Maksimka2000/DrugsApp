namespace DrugsApp.Domain.StudyModeFeature.Enums;

/// <summary>
/// Medical record section of a study patient that can be filled with random sample data.
/// </summary>
public enum StudyMedicalSection
{
    /// <summary>
    /// Catalog drug allergies.
    /// </summary>
    DrugAllergies,

    /// <summary>
    /// Catalog supplement allergies.
    /// </summary>
    SupplementAllergies,

    /// <summary>
    /// Catalog food allergies.
    /// </summary>
    FoodAllergies,

    /// <summary>
    /// Other (environmental) allergies from a curated list.
    /// </summary>
    OtherAllergies,

    /// <summary>
    /// Catalog health conditions.
    /// </summary>
    HealthConditions,

    /// <summary>
    /// Drug problems tied to catalog drug forms.
    /// </summary>
    DrugProblems,

    /// <summary>
    /// Ongoing regular drug schedules.
    /// </summary>
    RegularDrugs,

    /// <summary>
    /// Ongoing regular catalog supplement schedules.
    /// </summary>
    RegularSupplements,

    /// <summary>
    /// Own supplements (named mixes of catalog supplements).
    /// </summary>
    OwnSupplements,

    /// <summary>
    /// Ongoing regular food schedules.
    /// </summary>
    RegularFood,

    /// <summary>
    /// Meal times from a curated list.
    /// </summary>
    MealTimes
}
