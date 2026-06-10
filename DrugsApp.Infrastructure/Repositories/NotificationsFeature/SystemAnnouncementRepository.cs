using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Domain.NotificationsFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.NotificationsFeature;

public sealed class SystemAnnouncementRepository : ISystemAnnouncementRepository
{
    private readonly DrugsAppDbContext _context;

    public SystemAnnouncementRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public Task<SystemAnnouncement?> GetByIdAsync(Guid announcementId, CancellationToken cancellationToken)
    {
        return _context.SystemAnnouncements
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == announcementId, cancellationToken);
    }

    public async Task<List<SystemAnnouncement>> ListActiveAsync(DateTime asOfUtc, CancellationToken cancellationToken)
    {
        return await _context.SystemAnnouncements
            .AsNoTracking()
            .Where(x => x.ActiveFromUtc <= asOfUtc
                        && (x.ActiveToUtc == null || x.ActiveToUtc >= asOfUtc))
            .OrderBy(x => x.ActiveFromUtc)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}
