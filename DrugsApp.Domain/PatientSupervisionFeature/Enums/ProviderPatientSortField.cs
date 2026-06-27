namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Sortable fields for provider patient list queries.
/// </summary>
public enum ProviderPatientSortField
{
    /// <summary>
    /// Sort by relationship status.
    /// </summary>
    Status,

    /// <summary>
    /// Sort by date added.
    /// </summary>
    DateAdded,

    /// <summary>
    /// Sort by display name.
    /// </summary>
    DisplayName
}
