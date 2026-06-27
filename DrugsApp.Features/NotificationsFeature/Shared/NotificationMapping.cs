using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Features.NotificationsFeature.Shared;

namespace DrugsApp.Features.NotificationsFeature.Shared;

/// <summary>
/// Mapping helpers for notification entities.
/// </summary>
public static class NotificationMapping
{
    /// <summary>
    /// Maps a user notification domain entity to a list item DTO.
    /// </summary>
    /// <param name="notification">Notification entity.</param>
    /// <returns>List item DTO.</returns>
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
