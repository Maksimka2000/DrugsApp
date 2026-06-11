namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Lifecycle of a medical change proposal awaiting patient decision.
/// </summary>
public enum MedicalChangeProposalState
{
    Pending,
    Approved,
    Rejected,
    Invalidated
}
