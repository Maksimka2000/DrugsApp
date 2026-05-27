using System;

namespace DrugsApp.Domain.InteractionFeature.Enums;

/// <summary>
/// Describes the party category participating in an interaction.
/// </summary>
public enum InteractionType
{
    /// <summary>
    /// Represents an interaction type involving a single specific substance.
    /// This value is used to categorize interactions where an individual substance
    /// is the primary party in the interaction process.
    /// </summary>
    Substance = 0,

    /// <summary>
    /// Represents an interaction type involving a specific group of substances.
    /// This value is used to categorize interactions where a collection of substances
    /// is collectively associated with the interaction process.
    /// </summary>
    SubstanceGroup = 1,

    /// <summary>
    /// Represents an interaction type involving a specific food item.
    /// This value is used to categorize interactions where a single food item
    /// is directly related to the interaction process.
    /// </summary>
    Food = 2,

    /// <summary>
    /// Represents a group of related foods within the interaction system.
    /// This value is used to categorize interactions involving a collection of
    /// food items rather than a single food item.
    /// </summary>
    FoodGroup = 3,

    /// <summary>
    /// Represents a dietary supplement as a category of interaction party.
    /// </summary>
    Supplement = 4,

    /// <summary>
    /// Represents a group of related dietary supplements within the interaction system.
    /// This value is used to categorize interactions that involve a collection of
    /// supplements rather than an individual supplement.
    /// </summary>
    SupplementGroup = 5
}
