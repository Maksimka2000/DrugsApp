using DrugsApp.Features.NotificationsFeature.Shared;

namespace DrugsApp.Features.NotificationsFeature.GetUnreadNotificationCount;

public static class GetUnreadNotificationCountMapping
{
    public static UnreadNotificationCountResponse MapToResponse(
        this GetUnreadNotificationCountQuery query,
        int unreadCount,
        bool hasActionRequired)
        => new(unreadCount, hasActionRequired);
}
