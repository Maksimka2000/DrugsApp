using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Entities;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.AuthFeature;

public sealed class PendingEmailChangeRepository(DrugsAppDbContext _context) : IPendingEmailChangeRepository
{
    public async Task<PendingEmailChange?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;

        return await _context.PendingEmailChanges
            .FirstOrDefaultAsync(
                pending => pending.UserId == userId && pending.ExpiresAtUtc > now,
                cancellationToken);
    }

    public async Task<PendingEmailChange?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.PendingEmailChanges
            .FirstOrDefaultAsync(pending => pending.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(PendingEmailChange pendingEmailChange, CancellationToken cancellationToken)
    {
        await _context.PendingEmailChanges.AddAsync(pendingEmailChange, cancellationToken);
    }

    public async Task RemoveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        PendingEmailChange? pending = await _context.PendingEmailChanges
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (pending is null)
        {
            return;
        }

        _context.PendingEmailChanges.Remove(pending);
    }

    public void Remove(PendingEmailChange pendingEmailChange)
    {
        _context.PendingEmailChanges.Remove(pendingEmailChange);
    }
}
