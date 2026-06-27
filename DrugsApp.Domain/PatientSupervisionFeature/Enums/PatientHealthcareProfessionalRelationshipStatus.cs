namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Unified relationship status for patient healthcare professional list projections.
/// </summary>
public enum PatientHealthcareProfessionalRelationshipStatus
{
    /// <summary>
    /// Relationship request is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// Relationship is active.
    /// </summary>
    Active,

    /// <summary>
    /// Relationship request was rejected.
    /// </summary>
    Rejected,

    /// <summary>
    /// Relationship was cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Relationship request expired.
    /// </summary>
    Expired,

    /// <summary>
    /// Relationship was removed.
    /// </summary>
    Removed
}
