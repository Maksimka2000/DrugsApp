namespace DrugsApp.Domain.AllergiesFeature.Enums;

/// <summary>
/// Tri-state answer used by allergy summary questions.
/// </summary>
public enum AllergyAnswerState
{
    /// <summary>
    /// Affirmative answer.
    /// </summary>
    Yes = 1,

    /// <summary>
    /// Negative answer.
    /// </summary>
    No = 2,

    /// <summary>
    /// Unknown or not sure answer.
    /// </summary>
    DontKnow = 3
}
