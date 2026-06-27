namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Lifecycle of a medical change proposal awaiting patient decision.
/// </summary>
public enum MedicalChangeProposalState
{
    /// <summary>
    /// Awaiting decision.
    /// </summary>
    Pending,

    /// <summary>
    /// Approved by the decision maker.
    /// </summary>
    Approved,

    /// <summary>
    /// Rejected by the decision maker.
    /// </summary>
    Rejected,

    /// <summary>
    /// Invalidated due to supersession or other conditions.
    /// </summary>
    Invalidated
}
