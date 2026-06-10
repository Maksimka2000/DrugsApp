namespace DrugsApp.Domain.UserProfileFeature.Enums;

/// <summary>
/// Codes representing healthcare provider professions stored as strings.
/// </summary>
public enum HealthcareProviderProfession
{
    /// <summary>
    /// Medical doctor.
    /// </summary>
    doctor,

    /// <summary>
    /// Nurse profession.
    /// </summary>
    nurse,

    /// <summary>
    /// Pharmacist profession.
    /// </summary>
    pharmacist,

    /// <summary>
    /// Dietitian or nutrition specialist.
    /// </summary>
    dietitian,

    /// <summary>
    /// Therapist profession.
    /// </summary>
    therapist,

    /// <summary>
    /// Other profession not covered by predefined codes.
    /// </summary>
    other
}
