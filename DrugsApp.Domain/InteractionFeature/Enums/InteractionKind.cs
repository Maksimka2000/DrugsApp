namespace DrugsApp.Domain.InteractionFeature.Enums;

/// <summary>
/// High-level interaction kind used for query scoping.
/// </summary>
public enum InteractionKind
{
    /// <summary>
    /// Represents an interaction kind that involves substances.
    /// This value is used to scope queries related to interactions
    /// between chemical or biological substances and other entities such as drugs or food.
    /// </summary>
    Substance = 0,

    /// <summary>
    /// Represents an interaction kind that involves food.
    /// This value is used to scope queries related to interactions
    /// between food and other entities such as drugs, substances, or supplements.
    /// </summary>
    Food = 1,

    /// <summary>
    /// Represents an interaction kind that involves dietary supplements.
    /// This value is used to scope queries related to interactions
    /// between supplements and other entities such as drugs, substances, or food.
    /// </summary>
    Supplement = 2
}
