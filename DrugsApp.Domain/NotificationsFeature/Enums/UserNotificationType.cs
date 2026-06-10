namespace DrugsApp.Domain.NotificationsFeature.Enums;

/// <summary>
/// Normalized notification family stored in the user inbox projection. [description]
/// </summary>
public enum UserNotificationType
{
    SurveyReminder,
    HealthcareProviderSurveyReminder,
    SupervisionRequest,
    MedicalChangeProposal,
    SupervisionRemovedByDoctor,
    SupervisionRemovedByPatient,
    SystemAnnouncement,
    WeeklyHealthDataReminder,
    EmailVerificationPrompt,
    TwoFactorSuggestion,
    RegularDrugExpired,
    RegularFoodExpired,
    RegularSupplementExpired
}
