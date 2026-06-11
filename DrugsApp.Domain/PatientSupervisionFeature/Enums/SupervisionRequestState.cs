namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Lifecycle state of a supervision request.
/// </summary>
public enum SupervisionRequestState
{
    Pending,
    Approved,
    Rejected,
    Expired,
    Cancelled
}
