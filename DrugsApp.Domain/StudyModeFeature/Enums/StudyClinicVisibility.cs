namespace DrugsApp.Domain.StudyModeFeature.Enums;

/// <summary>
/// Who can see and manage a study clinic; persisted as a string.
/// </summary>
public enum StudyClinicVisibility
{
    /// <summary>
    /// Visible to every admin user holding the study mode permission.
    /// </summary>
    Public,

    /// <summary>
    /// Visible only to the admin user who created the clinic; deleted together with that user.
    /// </summary>
    Private
}
