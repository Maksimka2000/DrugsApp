using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Domain.NotificationsFeature.Enums;
using DrugsApp.Domain.NotificationsFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.NotificationsFeature;

public sealed class UserNotificationRepository : IUserNotificationRepository
{
    private readonly DrugsAppDbContext _context;

    public UserNotificationRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public Task<int> CountUnreadAsync(
        Guid userId,
        bool includeProviderAudience,
        CancellationToken cancellationToken)
    {
        IQueryable<UserNotification> query = ApplyAudienceFilter(
            _context.UserNotifications.AsNoTracking().Where(x => x.UserId == userId),
            includeProviderAudience);

        return query.Where(x => !x.IsRead).CountAsync(cancellationToken);
    }

    public Task<bool> ExistsUnreadActionRequiredAsync(
        Guid userId,
        bool includeProviderAudience,
        CancellationToken cancellationToken)
    {
        IQueryable<UserNotification> query = ApplyAudienceFilter(
            _context.UserNotifications.AsNoTracking().Where(x => x.UserId == userId),
            includeProviderAudience);

        return query.AnyAsync(
            x => !x.IsRead
                 && x.RequiresAction
                 && x.ActionState == NotificationActionState.Pending,
            cancellationToken);
    }

    public async Task<List<UserNotification>> ListAsync(
        Guid userId,
        bool includeProviderAudience,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<UserNotification> query = ApplyAudienceFilter(
            _context.UserNotifications.AsNoTracking().Where(x => x.UserId == userId),
            includeProviderAudience);

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountVisibleAsync(
        Guid userId,
        bool includeProviderAudience,
        CancellationToken cancellationToken)
    {
        IQueryable<UserNotification> query = ApplyAudienceFilter(
            _context.UserNotifications.AsNoTracking().Where(x => x.UserId == userId),
            includeProviderAudience);

        return query.CountAsync(cancellationToken);
    }

    public Task<UserNotification?> GetByIdForUserAsync(
        Guid notificationId,
        Guid userId,
        bool includeProviderAudience,
        CancellationToken cancellationToken)
    {
        IQueryable<UserNotification> query = ApplyAudienceFilter(
            _context.UserNotifications.AsNoTracking()
                .Where(x => x.Id == notificationId && x.UserId == userId),
            includeProviderAudience);

        return query.SingleOrDefaultAsync(cancellationToken);
    }

    public Task<UserNotification?> GetTrackedByIdForUserAsync(
        Guid notificationId,
        Guid userId,
        bool includeProviderAudience,
        CancellationToken cancellationToken)
    {
        IQueryable<UserNotification> query = ApplyAudienceFilter(
            _context.UserNotifications.Where(x => x.Id == notificationId && x.UserId == userId),
            includeProviderAudience);

        return query.SingleOrDefaultAsync(cancellationToken);
    }

    public Task<UserNotification?> GetTrackedBySourceEventKeyAsync(
        Guid userId,
        string sourceEventKey,
        CancellationToken cancellationToken)
    {
        return _context.UserNotifications
            .Where(x => x.UserId == userId && x.SourceEventKey == sourceEventKey)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(UserNotification notification, CancellationToken cancellationToken)
    {
        await _context.UserNotifications.AddAsync(notification, cancellationToken);
    }

    private static IQueryable<UserNotification> ApplyAudienceFilter(
        IQueryable<UserNotification> query,
        bool includeProviderAudience)
    {
        if (includeProviderAudience)
        {
            return query.Where(x =>
                x.AudienceScopeCode == NotificationAudienceScope.SharedRegisteredClient
                || x.AudienceScopeCode == NotificationAudienceScope.PatientOnly
                || x.AudienceScopeCode == NotificationAudienceScope.ProviderOnly);
        }

        return query.Where(x =>
            x.AudienceScopeCode == NotificationAudienceScope.SharedRegisteredClient
            || x.AudienceScopeCode == NotificationAudienceScope.PatientOnly);
    }
}
