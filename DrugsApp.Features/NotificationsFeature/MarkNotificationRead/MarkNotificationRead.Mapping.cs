using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Features.NotificationsFeature.Shared;

namespace DrugsApp.Features.NotificationsFeature.MarkNotificationRead;

public static class MarkNotificationReadMapping
{
    public static MarkNotificationReadResponse MapToMarkReadResponse(this UserNotification notification)
        => new(notification.Id, notification.IsRead, notification.ReadAtUtc);
}
