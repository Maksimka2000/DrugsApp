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

public sealed class ClientRegularSupplementRepository(DrugsAppDbContext _context) : IClientRegularSupplementRepository
{
    public async Task<List<ClientRegularSupplement>> SearchLiveByUserIdAsync(
        Guid userId,
        string? nameQuery,
        DateOnly? startDate,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularSupplement> query = BuildLiveQuery(userId, nameQuery, startDate);

        query = ApplyIncludes(query)
            .OrderBy(row => row.Status == RegularSupplementStatus.Scheduled ? 1 : 0)
            .ThenBy(row =>
                row.Source == RegularSupplementSource.Database
                    ? row.Supplement!.Name.ToLower()
                    : row.OwnSupplement!.Name.ToLower())
            .ThenBy(row => row.StartDate ?? DateOnly.MinValue)
            .ThenBy(row => row.Id);

        return await query
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ClientRegularSupplement>> ListActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularSupplement> query = _context.ClientRegularSupplements
            .Where(row => row.UserId == userId && row.Status == RegularSupplementStatus.Active);

        return await ApplyIncludes(query)
            .OrderBy(row => row.Id)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> ListActiveDatabaseSupplementIdsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _context.ClientRegularSupplements
            .AsNoTracking()
            .Where(row =>
                row.UserId == userId
                && row.Status == RegularSupplementStatus.Active
                && row.Source == RegularSupplementSource.Database
                && row.SupplementId != null)
            .Select(row => row.SupplementId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountLiveByUserIdAsync(
        Guid userId,
        string? nameQuery,
        DateOnly? startDate,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularSupplement> query = BuildLiveQuery(userId, nameQuery, startDate);

        return await query
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    public async Task<List<ClientRegularSupplement>> SearchHistoryByUserIdAsync(
        Guid userId,
        string? nameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularSupplement> query = BuildHistoryQuery(userId, nameQuery, startDate, endDate);

        query = ApplyIncludes(query)
            .OrderByDescending(row => row.ArchivedAtUtc ?? DateTime.MinValue)
            .ThenBy(row =>
                row.Source == RegularSupplementSource.Database
                    ? row.Supplement!.Name.ToLower()
                    : row.OwnSupplement!.Name.ToLower())
            .ThenBy(row => row.Id);

        return await query
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountHistoryByUserIdAsync(
        Guid userId,
        string? nameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularSupplement> query = BuildHistoryQuery(userId, nameQuery, startDate, endDate);

        return await query
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    public async Task<ClientRegularSupplement?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await ApplyIncludes(_context.ClientRegularSupplements
                .AsNoTracking()
                .Where(row => row.UserId == userId && row.Id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClientRegularSupplement?> GetTrackedByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await ApplyIncludes(_context.ClientRegularSupplements
                .Where(row => row.UserId == userId && row.Id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsLiveDuplicateForUserAsync(
        Guid userId,
        RegularSupplementSource source,
        Guid? supplementId,
        Guid? ownSupplementId,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularSupplement> query = _context.ClientRegularSupplements
            .AsNoTracking()
            .Where(row =>
                row.UserId == userId &&
                row.Source == source &&
                (row.Status == RegularSupplementStatus.Scheduled ||
                 row.Status == RegularSupplementStatus.Active));

        if (source == RegularSupplementSource.Database)
        {
            query = query.Where(row => row.SupplementId == supplementId);
        }
        else
        {
            query = query.Where(row => row.OwnSupplementId == ownSupplementId);
        }

        if (excludeId.HasValue)
        {
            query = query.Where(row => row.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(ClientRegularSupplement clientRegularSupplement, CancellationToken cancellationToken)
    {
        await _context.ClientRegularSupplements.AddAsync(clientRegularSupplement, cancellationToken);
    }

    public void Remove(ClientRegularSupplement clientRegularSupplement)
    {
        _context.ClientRegularSupplements.Remove(clientRegularSupplement);
    }

    public async Task<int> DeleteAllExpiredByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<ClientRegularSupplement> expiredRows = await _context.ClientRegularSupplements
            .Where(row => row.UserId == userId && row.Status == RegularSupplementStatus.Expired)
            .ToListAsync(cancellationToken);

        _context.ClientRegularSupplements.RemoveRange(expiredRows);

        return expiredRows.Count;
    }

    public async Task<List<ClientRegularSupplement>> FindEndedLiveRowsAsync(
        DateOnly asOfDate,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularSupplement> query = _context.ClientRegularSupplements
            .Where(row =>
                (row.Status == RegularSupplementStatus.Scheduled ||
                 row.Status == RegularSupplementStatus.Active) &&
                row.EndDate != null &&
                row.EndDate < asOfDate);

        query = ApplyIncludes(query);

        return await query
            .OrderBy(row => row.EndDate)
            .ThenBy(row => row.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<ClientRegularSupplement> BuildLiveQuery(
        Guid userId,
        string? nameQuery,
        DateOnly? startDate)
    {
        IQueryable<ClientRegularSupplement> query = _context.ClientRegularSupplements
            .Where(row =>
                row.UserId == userId &&
                (row.Status == RegularSupplementStatus.Scheduled ||
                 row.Status == RegularSupplementStatus.Active));

        query = ApplyNameFilter(query, nameQuery);

        if (startDate.HasValue)
        {
            query = query.Where(row => row.StartDate == startDate.Value);
        }

        return query;
    }

    private IQueryable<ClientRegularSupplement> BuildHistoryQuery(
        Guid userId,
        string? nameQuery,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        IQueryable<ClientRegularSupplement> query = _context.ClientRegularSupplements
            .Where(row => row.UserId == userId && row.Status == RegularSupplementStatus.Expired);

        query = ApplyNameFilter(query, nameQuery);

        if (startDate.HasValue)
        {
            query = query.Where(row => row.StartDate == startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(row => row.EndDate == endDate.Value);
        }

        return query;
    }

    private static IQueryable<ClientRegularSupplement> ApplyNameFilter(
        IQueryable<ClientRegularSupplement> query,
        string? nameQuery)
    {
        if (string.IsNullOrWhiteSpace(nameQuery))
        {
            return query;
        }

        string trimmedQuery = nameQuery.Trim();
        string containsPattern = $"%{trimmedQuery}%";

        return query.Where(row =>
            row.Supplement != null && EF.Functions.ILike(row.Supplement.Name, containsPattern) ||
            row.OwnSupplement != null && EF.Functions.ILike(row.OwnSupplement.Name, containsPattern));
    }

    private static IQueryable<ClientRegularSupplement> ApplyIncludes(IQueryable<ClientRegularSupplement> query) =>
        query
            .Include(row => row.IntakeTimes)
            .Include(row => row.Supplement)
            .Include(row => row.OwnSupplement)
                .ThenInclude(ownSupplement => ownSupplement!.Items)
                    .ThenInclude(item => item.Supplement);
}
