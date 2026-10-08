using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Enums;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.RegularFoodFeature;

public sealed class ClientRegularFoodRepository(DrugsAppDbContext _context) : IClientRegularFoodRepository
{
    public async Task<List<ClientRegularFood>> SearchLiveByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularFood> query = BuildLiveQuery(userId, foodNameQuery, startDate);

        query = ApplyIncludes(query)
            .OrderBy(row => row.Status == RegularFoodStatus.Scheduled ? 1 : 0)
            .ThenBy(row => row.Food.Name.ToLower())
            .ThenBy(row => row.StartDate ?? DateOnly.MinValue)
            .ThenBy(row => row.Id);

        return await query
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> ListActiveFoodIdsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _context.Set<ClientRegularFood>()
            .AsNoTracking()
            .Where(row => row.UserId == userId && row.Status == RegularFoodStatus.Active)
            .Select(row => row.FoodId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountLiveByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularFood> query = BuildLiveQuery(userId, foodNameQuery, startDate);

        return await query
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    public async Task<List<ClientRegularFood>> SearchHistoryByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularFood> query = BuildHistoryQuery(userId, foodNameQuery, startDate, endDate);

        query = ApplyIncludes(query)
            .OrderByDescending(row => row.ArchivedAtUtc ?? DateTime.MinValue)
            .ThenBy(row => row.Food.Name.ToLower())
            .ThenBy(row => row.Id);

        return await query
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountHistoryByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularFood> query = BuildHistoryQuery(userId, foodNameQuery, startDate, endDate);

        return await query
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    public async Task<ClientRegularFood?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        bool track,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularFood> query = _context.Set<ClientRegularFood>()
            .Where(row => row.UserId == userId && row.Id == id);

        query = ApplyIncludes(query);

        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsLiveForUserWithFoodIdAsync(
        Guid userId,
        Guid foodId,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularFood> query = _context.Set<ClientRegularFood>()
            .AsNoTracking()
            .Where(row =>
                row.UserId == userId &&
                row.FoodId == foodId &&
                (row.Status == RegularFoodStatus.Scheduled || row.Status == RegularFoodStatus.Active));

        if (excludeId.HasValue)
        {
            query = query.Where(row => row.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(ClientRegularFood clientRegularFood, CancellationToken cancellationToken)
    {
        await _context.Set<ClientRegularFood>().AddAsync(clientRegularFood, cancellationToken);
    }

    public void Remove(ClientRegularFood clientRegularFood)
    {
        _context.Set<ClientRegularFood>().Remove(clientRegularFood);
    }

    public async Task<int> RemoveExpiredByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<ClientRegularFood> expiredRows = await _context.Set<ClientRegularFood>()
            .Where(row => row.UserId == userId && row.Status == RegularFoodStatus.Expired)
            .ToListAsync(cancellationToken);

        _context.Set<ClientRegularFood>().RemoveRange(expiredRows);

        return expiredRows.Count;
    }

    public async Task<List<ClientRegularFood>> ListEndedLiveRowsAsync(
        DateOnly asOfDate,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularFood> query = _context.Set<ClientRegularFood>()
            .Where(row =>
                (row.Status == RegularFoodStatus.Scheduled || row.Status == RegularFoodStatus.Active) &&
                row.EndDate != null &&
                row.EndDate < asOfDate);

        query = ApplyIncludes(query);

        return await query
            .OrderBy(row => row.EndDate)
            .ThenBy(row => row.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<ClientRegularFood> BuildLiveQuery(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate)
    {
        IQueryable<ClientRegularFood> query = _context.Set<ClientRegularFood>()
            .Where(row =>
                row.UserId == userId &&
                (row.Status == RegularFoodStatus.Scheduled || row.Status == RegularFoodStatus.Active));

        query = ApplyFoodNameFilter(query, foodNameQuery);

        if (startDate.HasValue)
        {
            query = query.Where(row => row.StartDate == startDate.Value);
        }

        return query;
    }

    private IQueryable<ClientRegularFood> BuildHistoryQuery(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        IQueryable<ClientRegularFood> query = _context.Set<ClientRegularFood>()
            .Where(row => row.UserId == userId && row.Status == RegularFoodStatus.Expired);

        query = ApplyFoodNameFilter(query, foodNameQuery);

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

    private static IQueryable<ClientRegularFood> ApplyFoodNameFilter(
        IQueryable<ClientRegularFood> query,
        string? foodNameQuery)
    {
        if (string.IsNullOrWhiteSpace(foodNameQuery))
        {
            return query;
        }

        string trimmedQuery = foodNameQuery.Trim();
        string containsPattern = $"%{trimmedQuery}%";

        return query.Where(row =>
            EF.Functions.ILike(row.Food.Name, containsPattern) ||
            EF.Functions.ILike(row.Food.SearchText, containsPattern));
    }

    private static IQueryable<ClientRegularFood> ApplyIncludes(IQueryable<ClientRegularFood> query) =>
        query
            .Include(row => row.IntakeTimes)
            .Include(row => row.Food);
}
