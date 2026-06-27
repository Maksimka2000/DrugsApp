namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Searchable fields for provider patient list queries.
/// </summary>
public enum ProviderPatientSearchField
{
    /// <summary>
    /// No specific search field applied.
    /// </summary>
    None = 0,

    /// <summary>
    /// Search by email address.
    /// </summary>
    Email = 1,

    /// <summary>
    /// Search by display name.
    /// </summary>
    DisplayName = 2
}
