namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Unified relationship status for patient healthcare professional list projections.
/// </summary>
public enum PatientHealthcareProfessionalRelationshipStatus
{
    Pending,
    Active,
    Rejected,
    Cancelled,
    Expired,
    Removed
}
