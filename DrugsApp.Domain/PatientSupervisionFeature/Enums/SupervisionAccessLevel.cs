namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Patient-controlled access level for a supervision connection.
/// </summary>
public enum SupervisionAccessLevel
{
    /// <summary>
    /// Full view and modification rights.
    /// </summary>
    ViewAndModify,

    /// <summary>
    /// View-only access.
    /// </summary>
    ViewOnly,

    /// <summary>
    /// Limited view with restricted details.
    /// </summary>
    RestrictedView,

    /// <summary>
    /// Access fully blocked.
    /// </summary>
    BlockAccess
}
