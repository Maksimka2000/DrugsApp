namespace DrugsApp.Domain.HealthConditionFeature.Entities;

/// <summary>
/// Indicates whether a user health condition comes from the catalog or a custom entry.
/// </summary>
public enum HealthConditionSource
{
    /// <summary>
    /// Health condition references a catalog entry.
    /// </summary>
    Database,

    /// <summary>
    /// Health condition is a user-provided custom name.
    /// </summary>
    Custom
}
