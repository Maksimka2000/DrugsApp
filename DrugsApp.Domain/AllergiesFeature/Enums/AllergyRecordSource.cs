namespace DrugsApp.Domain.AllergiesFeature.Enums;

/// <summary>
/// Origin of an allergy record.
/// </summary>
public enum AllergyRecordSource
{
    /// <summary>
    /// Linked to a catalog record.
    /// </summary>
    Database = 1,

    /// <summary>
    /// User-provided custom entry.
    /// </summary>
    Custom = 2
}
