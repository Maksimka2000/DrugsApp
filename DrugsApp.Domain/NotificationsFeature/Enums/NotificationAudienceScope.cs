namespace DrugsApp.Domain.NotificationsFeature.Enums;

/// <summary>
/// Inbox visibility group used for role-based filtering. [description]
/// </summary>
public enum NotificationAudienceScope
{
    /// <summary>
    /// Visible to all registered clients.
    /// </summary>
    SharedRegisteredClient,

    /// <summary>
    /// Visible only to patient users.
    /// </summary>
    PatientOnly,

    /// <summary>
    /// Visible only to healthcare providers.
    /// </summary>
    ProviderOnly
}
