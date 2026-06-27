namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Source of data for a proposal item.
/// </summary>
public enum MedicalChangeProposalSource
{
    /// <summary>
    /// Derived from existing database records.
    /// </summary>
    Database,

    /// <summary>
    /// Provided as custom input.
    /// </summary>
    Custom,

    /// <summary>
    /// Derived from user profile data.
    /// </summary>
    ProfileData
}
