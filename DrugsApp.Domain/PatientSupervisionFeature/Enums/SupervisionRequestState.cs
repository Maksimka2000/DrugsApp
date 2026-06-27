namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Lifecycle state of a supervision request.
/// </summary>
public enum SupervisionRequestState
{
    /// <summary>
    /// Awaiting approval.
    /// </summary>
    Pending,

    /// <summary>
    /// Request approved.
    /// </summary>
    Approved,

    /// <summary>
    /// Request rejected.
    /// </summary>
    Rejected,

    /// <summary>
    /// Request expired before action.
    /// </summary>
    Expired,

    /// <summary>
    /// Request cancelled by a party.
    /// </summary>
    Cancelled
}
