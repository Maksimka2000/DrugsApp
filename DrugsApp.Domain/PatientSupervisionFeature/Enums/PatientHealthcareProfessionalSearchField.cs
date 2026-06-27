namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Searchable fields for patient healthcare professional list queries.
/// </summary>
public enum PatientHealthcareProfessionalSearchField
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
    DisplayName = 2,

    /// <summary>
    /// Search by profession label.
    /// </summary>
    ProfessionLabel = 3,

    /// <summary>
    /// Search by workplace label.
    /// </summary>
    WorkplaceLabel = 4
}
