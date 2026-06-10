namespace DrugsApp.Domain.NotificationsFeature.Enums;

/// <summary>
/// Inbox visibility group used for role-based filtering. [description]
/// </summary>
public enum NotificationAudienceScope
{
    SharedRegisteredClient,
    PatientOnly,
    ProviderOnly
}
