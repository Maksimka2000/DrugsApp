namespace DrugsApp.Domain.InteractionFeature.Enums;

/// <summary>
/// Sorting options for interaction search.
/// </summary>
public enum InteractionSortBy
{
    /// <summary>
    /// Specifies that interactions should be sorted based on their severity level.
    /// This option prioritizes the importance or intensity of interactions when ordering results.
    /// </summary>
    Severity = 0,

    /// <summary>
    /// Specifies that interactions should be sorted by their creation timestamp.
    /// </summary>
    CreatedAt = 1,

    /// <summary>
    /// Specifies that interactions should be sorted based on their last update timestamp.
    /// This option prioritizes the most recently updated interactions when ordering results.
    /// </summary>
    UpdatedAt = 2
}
