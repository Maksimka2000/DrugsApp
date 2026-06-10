using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Entities;

namespace DrugsApp.Domain.NotificationsFeature.Repositories;

/// <summary>
/// Repository contract for system announcement source rows.
/// </summary>
public interface ISystemAnnouncementRepository
{
    Task<SystemAnnouncement?> GetByIdAsync(Guid announcementId, CancellationToken cancellationToken);

    Task<List<SystemAnnouncement>> ListActiveAsync(DateTime asOfUtc, CancellationToken cancellationToken);
}
