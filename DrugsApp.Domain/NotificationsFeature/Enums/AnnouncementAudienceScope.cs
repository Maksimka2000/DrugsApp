namespace DrugsApp.Domain.NotificationsFeature.Enums;

/// <summary>
/// Audience visibility scope for system announcement fan-out. [description]
/// </summary>
public enum AnnouncementAudienceScope
{
    AllRegisteredClients,
    PatientOnly,
    ProviderOnly
}
