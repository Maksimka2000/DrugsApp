namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Patient-controlled access level for a supervision connection.
/// </summary>
public enum SupervisionAccessLevel
{
    ViewAndModify,
    ViewOnly,
    RestrictedView,
    BlockAccess
}
