using System.Collections.Generic;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Domain.ActiveSubstanceFeature.Entities;

/// <summary>
/// Represents an active substance definition used to describe drug composition.
/// </summary>
public sealed class ActiveSubstance : BaseEntity
{
    

    private ActiveSubstance() : base()
    {
    }

    // Navigation properties

    /// <summary>
    /// Optional oral medication intake guidelines associated with this active substance.
    /// </summary>
    public OralMedicationIntakeGuidelines? OralMedicationIntakeGuidelines { get; private set; }

    private readonly List<Drug> _drugs = new();
    /// <summary>
    /// Drug aggregates referencing this active substance.
    /// </summary>
    public IReadOnlyCollection<Drug> Drugs => _drugs;




    // Properties

    /// <summary>
    /// The display name of the active substance.
    /// </summary>
    public string ActiveSubstanceName { get; private set; } = string.Empty;

    /// <summary>
    /// Creates a new active substance aggregate.
    /// </summary>
    /// <param name="activeSubstanceName">Active substance name.</param>
    /// <returns>A new <see cref="ActiveSubstance"/> instance.</returns>
    public static ActiveSubstance Create(string activeSubstanceName) => new()
    {
        ActiveSubstanceName = activeSubstanceName.Trim()
    };

    /// <summary>
    /// Updates the active substance name.
    /// </summary>
    /// <param name="activeSubstanceName">New active substance name.</param>
    public void UpdateActiveSubstanceName(string activeSubstanceName)
    {
        string trimmedName = activeSubstanceName.Trim();

        if (ActiveSubstanceName == trimmedName)
        {
            return;
        }

        ActiveSubstanceName = trimmedName;
    }

    /// <summary>
    /// Initializes oral medication intake guidelines for this active substance (0..1).
    /// </summary>
    public void InitializeOralMedicationIntakeGuidelines(
        FoodIntakeType foodIntakeType,
        int? waitBeforeMinutes,
        int? waitAfterMinutes,
        string? mealNotes,
        string? avoidFoodDrug,
        string? avoidFoodTherapy,
        string? liquidNotes,
        bool? liquidInstructions,
        bool? recommendedMorning,
        bool? recommendedEvening)
    {
        if (OralMedicationIntakeGuidelines is not null)
        {
            return;
        }

        OralMedicationIntakeGuidelines = OralMedicationIntakeGuidelines.Create(
            activeSubstanceId: Id,
            foodIntakeType: foodIntakeType,
            waitBeforeMinutes: waitBeforeMinutes,
            waitAfterMinutes: waitAfterMinutes,
            mealNotes: mealNotes,
            avoidFoodDrug: avoidFoodDrug,
            avoidFoodTherapy: avoidFoodTherapy,
            liquidNotes: liquidNotes,
            liquidInstructions: liquidInstructions,
            recommendedMorning: recommendedMorning,
            recommendedEvening: recommendedEvening);
    }
}


