using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Features.NotificationsFeature.Shared;

namespace DrugsApp.Features.NotificationsFeature.Shared;

public static class NotificationMapping
{
    public static NotificationListItemDto MapToListItemDto(this UserNotification notification)
    {
        return new NotificationListItemDto(
            notification.Id,
            notification.TypeCode.ToString(),
            notification.PrimaryTagCode.ToString(),
            notification.AudienceScopeCode.ToString(),
            notification.Title,
            notification.SummaryText,
            notification.CreatedAtUtc,
            notification.IsRead,
            notification.RequiresAction,
            notification.ActionState.ToString(),
            notification.DeepLinkPath);
    }
}
