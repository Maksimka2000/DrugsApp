namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Sortable fields for patient healthcare professional list queries.
/// </summary>
public enum PatientHealthcareProfessionalSortField
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
