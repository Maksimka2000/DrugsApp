using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using DrugsApp.Domain.MealTimeManagementFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.MealTimeManagementFeature;

public sealed class ClientMealTimeRepository : IClientMealTimeRepository
{
    private readonly DrugsAppDbContext _context;

    public ClientMealTimeRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClientMealTime>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _context.ClientMealTimes
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.IntakeTime)
            .ThenBy(x => x.NormalizedMealName)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClientMealTime?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        bool tracking,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientMealTime> query = _context.ClientMealTimes
            .Where(x => x.UserId == userId && x.Id == id);

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNormalizedMealNameAsync(
        Guid userId,
        string normalizedMealName,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientMealTime> query = _context.ClientMealTimes
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.NormalizedMealName == normalizedMealName);

        if (excludeId is not null)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithIntakeTimeAsync(
        Guid userId,
        TimeOnly intakeTime,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientMealTime> query = _context.ClientMealTimes
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IntakeTime == intakeTime);

        if (excludeId is not null)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(ClientMealTime mealTime, CancellationToken cancellationToken)
    {
        await _context.ClientMealTimes.AddAsync(mealTime, cancellationToken);
    }

    public void Remove(ClientMealTime mealTime)
    {
        _context.ClientMealTimes.Remove(mealTime);
    }
}
