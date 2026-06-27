using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Features.NotificationsFeature.Shared;

namespace DrugsApp.Features.NotificationsFeature.MarkNotificationRead;

/// <summary>
/// Mapping helpers for marking notifications as read.
/// </summary>
public static class MarkNotificationReadMapping
{
    /// <summary>
    /// Maps a notification entity to a mark-read response.
    /// </summary>
    /// <param name="notification">Notification entity.</param>
    /// <returns>Mark read response DTO.</returns>
    public static MarkNotificationReadResponse MapToMarkReadResponse(this UserNotification notification)
        => new(notification.Id, notification.IsRead, notification.ReadAtUtc);
}
