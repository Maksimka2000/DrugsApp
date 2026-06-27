using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Features.NotificationsFeature.Shared;

namespace DrugsApp.Features.NotificationsFeature.ListNotifications;

/// <summary>
/// Mapping helpers for listing notifications.
/// </summary>
public static class ListNotificationsMapping
{
    /// <summary>
    /// Builds a query for listing notifications.
    /// </summary>
    /// <param name="request">Incoming list request.</param>
    /// <param name="userId">User identifier.</param>
    /// <param name="includeProviderAudience">Whether to include provider audience items.</param>
    /// <returns>Query with paging.</returns>
    public static ListNotificationsQuery MapToQuery(
        this ListNotificationsRequest request,
        System.Guid userId,
        bool includeProviderAudience)
        => new(userId, includeProviderAudience, request.Skip, request.Take);

    /// <summary>
    /// Maps notification entities to a list response with paging info.
    /// </summary>
    /// <param name="items">Notification entities.</param>
    /// <param name="totalCount">Total notifications count.</param>
    /// <param name="skip">Skip value used in query.</param>
    /// <returns>List response DTO.</returns>
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
