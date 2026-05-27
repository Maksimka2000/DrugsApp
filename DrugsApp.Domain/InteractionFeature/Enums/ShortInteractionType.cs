namespace DrugsApp.Domain.InteractionFeature.Enums;

/// <summary>
/// Atomic interaction types used in expanded projection rows.
/// </summary>
public enum ShortInteractionType
{
    /// <summary>
    /// Represents a type of interaction involving chemical or pharmacological substances.
    /// This member identifies interactions that pertain specifically to active compounds
    /// or drugs used for therapeutic, diagnostic, or preventive purposes.
    /// </summary>
    Substance = 0,

    /// <summary>
    /// Represents an interaction involving food.
    /// This type is used to specify scenarios where food is a participant in atomic interaction pairs.
    /// </summary>
    Food = 1,

    /// <summary>
    /// Represents a type of interaction involving dietary supplements.
    /// This member identifies interactions that pertain specifically to
    /// non-prescription substances such as vitamins, minerals, or herbal
    /// supplements used to complement a diet.
    /// </summary>
    Supplement = 2
}
