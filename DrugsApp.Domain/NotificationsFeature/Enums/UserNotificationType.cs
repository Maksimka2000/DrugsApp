namespace DrugsApp.Domain.NotificationsFeature.Enums;

/// <summary>
/// Normalized notification family stored in the user inbox projection. [description]
/// </summary>
public enum UserNotificationType
{
    /// <summary>
    /// Reminder for a patient survey.
    /// </summary>
    SurveyReminder,

    /// <summary>
    /// Reminder for a healthcare provider survey.
    /// </summary>
    HealthcareProviderSurveyReminder,

    /// <summary>
    /// Notification about a supervision request.
    /// </summary>
    SupervisionRequest,

    /// <summary>
    /// Notification about a medical change proposal.
    /// </summary>
    MedicalChangeProposal,

    /// <summary>
    /// Notification that supervision was removed by a doctor.
    /// </summary>
    SupervisionRemovedByDoctor,

    /// <summary>
    /// Notification that supervision was removed by a patient.
    /// </summary>
    SupervisionRemovedByPatient,

    /// <summary>
    /// Notification that a supervision access level changed.
    /// </summary>
    SupervisionAccessLevelChanged,

    /// <summary>
    /// General system announcement.
    /// </summary>
    SystemAnnouncement,

    /// <summary>
    /// Weekly reminder to provide health data.
    /// </summary>
    WeeklyHealthDataReminder,

    /// <summary>
    /// Prompt to verify email.
    /// </summary>
    EmailVerificationPrompt,

    /// <summary>
    /// Suggest enabling two-factor authentication.
    /// </summary>
    TwoFactorSuggestion,

    /// <summary>
    /// Notification that a regular drug schedule expired.
    /// </summary>
    RegularDrugExpired,

    /// <summary>
    /// Notification that a regular food schedule expired.
    /// </summary>
    RegularFoodExpired,

    /// <summary>
    /// Notification that a regular supplement schedule expired.
    /// </summary>
    RegularSupplementExpired
}
