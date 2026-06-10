using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.RegularSupplementsFeature;

public sealed class ClientOwnSupplementRepository(DrugsAppDbContext _context) : IClientOwnSupplementRepository
{
    public async Task<List<ClientOwnSupplement>> ListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await ApplyMemberIncludes(_context.ClientOwnSupplements
                .AsNoTracking()
                .Where(row => row.UserId == userId))
            .OrderBy(row => row.NormalizedName)
            .ThenBy(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClientOwnSupplement?> GetByIdForUserWithMembersAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await ApplyMemberIncludes(_context.ClientOwnSupplements
                .AsNoTracking()
                .Where(row => row.UserId == userId && row.Id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClientOwnSupplement?> GetTrackedByIdForUserWithMembersAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await ApplyMemberIncludes(_context.ClientOwnSupplements
                .Where(row => row.UserId == userId && row.Id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClientOwnSupplement?> FindByNormalizedNameForUserAsync(
        Guid userId,
        string normalizedName,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientOwnSupplement> query = _context.ClientOwnSupplements
            .AsNoTracking()
            .Where(row => row.UserId == userId && row.NormalizedName == normalizedName);

        if (excludeId.HasValue)
        {
            query = query.Where(row => row.Id != excludeId.Value);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.ClientOwnSupplements
            .AsNoTracking()
            .CountAsync(row => row.UserId == userId, cancellationToken);
    }

    public async Task<(int LiveCount, int ExpiredCount)> CountReferencingRegularSupplementsAsync(
        Guid ownSupplementId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularSupplement> query = _context.ClientRegularSupplements
            .AsNoTracking()
            .Where(row => row.OwnSupplementId == ownSupplementId);

        int liveCount = await query.CountAsync(
            row =>
                row.Status == RegularSupplementStatus.Scheduled ||
                row.Status == RegularSupplementStatus.Active,
            cancellationToken);

        int expiredCount = await query.CountAsync(
            row => row.Status == RegularSupplementStatus.Expired,
            cancellationToken);

        return (liveCount, expiredCount);
    }

    public async Task AddAsync(ClientOwnSupplement clientOwnSupplement, CancellationToken cancellationToken)
    {
        await _context.ClientOwnSupplements.AddAsync(clientOwnSupplement, cancellationToken);
    }

    public void Remove(ClientOwnSupplement clientOwnSupplement)
    {
        _context.ClientOwnSupplements.Remove(clientOwnSupplement);
    }

    private static IQueryable<ClientOwnSupplement> ApplyMemberIncludes(IQueryable<ClientOwnSupplement> query) =>
        query
            .Include(row => row.Items)
                .ThenInclude(item => item.Supplement);
}
