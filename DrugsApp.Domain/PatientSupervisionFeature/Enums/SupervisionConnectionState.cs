namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Lifecycle state of a supervision connection.
/// </summary>
public enum SupervisionConnectionState
{
    /// <summary>
    /// Connection is active.
    /// </summary>
    Active,

    /// <summary>
    /// Connection has been removed.
    /// </summary>
    Removed
}
