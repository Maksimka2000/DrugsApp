using DrugsApp.Features.NotificationsFeature.Shared;

namespace DrugsApp.Features.NotificationsFeature.GetUnreadNotificationCount;

/// <summary>
/// Mapping helpers for unread notification count.
/// </summary>
public static class GetUnreadNotificationCountMapping
{
    /// <summary>
    /// Maps count and action flags to a response DTO.
    /// </summary>
    /// <param name="query">Unread count query.</param>
    /// <param name="unreadCount">Unread notification count.</param>
    /// <param name="hasActionRequired">Whether any notification requires action.</param>
    /// <returns>Unread count response DTO.</returns>
    public static UnreadNotificationCountResponse MapToResponse(
        this GetUnreadNotificationCountQuery query,
        int unreadCount,
        bool hasActionRequired)
        => new(unreadCount, hasActionRequired);
}
