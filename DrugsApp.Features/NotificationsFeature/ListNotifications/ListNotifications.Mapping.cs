using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Features.NotificationsFeature.Shared;

namespace DrugsApp.Features.NotificationsFeature.ListNotifications;

public static class ListNotificationsMapping
{
    public static ListNotificationsQuery MapToQuery(
        this ListNotificationsRequest request,
        System.Guid userId,
        bool includeProviderAudience)
        => new(userId, includeProviderAudience, request.Skip, request.Take);

    public static NotificationListResponse MapToResponse(
        this IReadOnlyList<UserNotification> items,
        int totalCount,
        int skip)
    {
        var mappedItems = items.Select(item => item.MapToListItemDto()).ToList();
        bool hasMore = skip + mappedItems.Count < totalCount;
        return new NotificationListResponse(mappedItems, totalCount, hasMore);
    }
}
