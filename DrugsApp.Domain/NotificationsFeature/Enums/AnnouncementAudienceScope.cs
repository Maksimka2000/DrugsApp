namespace DrugsApp.Domain.NotificationsFeature.Enums;

/// <summary>
/// Audience visibility scope for system announcement fan-out. [description]
/// </summary>
public enum AnnouncementAudienceScope
{
    /// <summary>
    /// Announcement targets all registered clients.
    /// </summary>
    AllRegisteredClients,

    /// <summary>
    /// Announcement targets patient users only.
    /// </summary>
    PatientOnly,

    /// <summary>
    /// Announcement targets healthcare provider users only.
    /// </summary>
    ProviderOnly
}
