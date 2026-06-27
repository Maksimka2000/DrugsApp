namespace DrugsApp.Domain.NotificationsFeature.Enums;

/// <summary>
/// Action or resolution badge synchronized from source domains when applicable. [description]
/// </summary>
public enum NotificationActionState
{
    /// <summary>
    /// Awaiting user or system action.
    /// </summary>
    Pending,

    /// <summary>
    /// Action request was approved.
    /// </summary>
    Approved,

    /// <summary>
    /// Action request was rejected.
    /// </summary>
    Rejected,

    /// <summary>
    /// Action request was completed.
    /// </summary>
    Completed,

    /// <summary>
    /// Informational message only.
    /// </summary>
    Informational,

    /// <summary>
    /// Action request expired before completion.
    /// </summary>
    Expired
}
