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

    /// <summary>
    /// Marker indicating whether the substance has an alcohol interaction.
    /// </summary>
    public bool HasAlcoholInteraction { get; private set; }

    /// <summary>
    /// Optional override text for alcohol interaction specific guidance.
    /// </summary>
    public ActiveSubstanceAlcoholInteractionOverride? AlcoholInteractionOverride { get; private set; }




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
        ActiveSubstanceName = activeSubstanceName.Trim(),
        HasAlcoholInteraction = false
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

    /// <summary>
    /// Updates the alcohol interaction marker.
    /// </summary>
    /// <param name="hasAlcoholInteraction">Indicates whether alcohol interaction is present.</param>
    public void SetHasAlcoholInteraction(bool hasAlcoholInteraction)
    {
        HasAlcoholInteraction = hasAlcoholInteraction;
    }

    /// <summary>
    /// Creates or updates the alcohol interaction override text and enforces the alcohol marker.
    /// </summary>
    /// <param name="alcoholSpecialText">Override text describing alcohol interaction specifics.</param>
    public void UpsertAlcoholInteractionOverride(string alcoholSpecialText)
    {
        string normalizedText = NormalizeRequiredText(alcoholSpecialText);

        if (string.IsNullOrEmpty(normalizedText))
        {
            return;
        }

        HasAlcoholInteraction = true;

        if (AlcoholInteractionOverride is null)
        {
            AlcoholInteractionOverride = ActiveSubstanceAlcoholInteractionOverride.Create(
                Id,
                normalizedText);

            return;
        }

        AlcoholInteractionOverride.UpdateAlcoholSpecialText(normalizedText);
    }

    /// <summary>
    /// Removes the alcohol interaction override text.
    /// </summary>
    public void ClearAlcoholInteractionOverride()
    {
        AlcoholInteractionOverride = null;
    }

    private static string NormalizeRequiredText(string value)
    {
        string trimmed = value.Trim();
        return string.IsNullOrEmpty(trimmed)
            ? string.Empty
            : trimmed;
    }
}


