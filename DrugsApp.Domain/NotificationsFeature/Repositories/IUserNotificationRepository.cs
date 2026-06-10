using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Entities;

namespace DrugsApp.Domain.NotificationsFeature.Repositories;

/// <summary>
/// Repository contract for user inbox projection rows.
/// </summary>
public interface IUserNotificationRepository
{
    Task<int> CountUnreadAsync(
        Guid userId,
        bool includeProviderAudience,
        CancellationToken cancellationToken);

    Task<bool> ExistsUnreadActionRequiredAsync(
        Guid userId,
        bool includeProviderAudience,
        CancellationToken cancellationToken);

    Task<List<UserNotification>> ListAsync(
        Guid userId,
        bool includeProviderAudience,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountVisibleAsync(
        Guid userId,
        bool includeProviderAudience,
        CancellationToken cancellationToken);

    Task<UserNotification?> GetByIdForUserAsync(
        Guid notificationId,
        Guid userId,
        bool includeProviderAudience,
        CancellationToken cancellationToken);

    Task<UserNotification?> GetTrackedByIdForUserAsync(
        Guid notificationId,
        Guid userId,
        bool includeProviderAudience,
        CancellationToken cancellationToken);

    Task<UserNotification?> GetTrackedBySourceEventKeyAsync(
        Guid userId,
        string sourceEventKey,
        CancellationToken cancellationToken);

    Task AddAsync(UserNotification notification, CancellationToken cancellationToken);
}
