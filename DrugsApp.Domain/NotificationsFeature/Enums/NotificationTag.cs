namespace DrugsApp.Domain.NotificationsFeature.Enums;

/// <summary>
/// UI tag badge grouping for inbox cards and detail panels. [description]
/// </summary>
public enum NotificationTag
{
    /// <summary>
    /// Concerns doctor–patient relationship changes.
    /// </summary>
    DoctorPatient,

    /// <summary>
    /// Relates to medical data updates.
    /// </summary>
    MedicalData,

    /// <summary>
    /// General system information.
    /// </summary>
    SystemInformation,

    /// <summary>
    /// Security-related notification.
    /// </summary>
    Security
}
