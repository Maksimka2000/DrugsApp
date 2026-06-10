namespace DrugsApp.Domain.NotificationsFeature.Enums;

/// <summary>
/// Action or resolution badge synchronized from source domains when applicable. [description]
/// </summary>
public enum NotificationActionState
{
    Pending,
    Approved,
    Rejected,
    Completed,
    Informational,
    Expired
}
