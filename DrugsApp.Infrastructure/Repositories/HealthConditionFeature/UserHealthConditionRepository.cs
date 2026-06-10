using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.HealthConditionFeature;

public sealed class UserHealthConditionRepository : IUserHealthConditionRepository
{
    private const double SimilarityThreshold = 0.35;

    private readonly DrugsAppDbContext _context;

    public UserHealthConditionRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserHealthCondition>> SearchAsync(
        Guid userId,
        string? displayNameQuery,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<UserHealthCondition> query = _context.UserHealthConditions
            .AsNoTracking()
            .Include(x => x.HealthCondition)
            .Where(x => x.UserId == userId);

        query = ApplyDisplayNameFilter(query, displayNameQuery);

        query = ApplySort(query);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        Guid userId,
        string? displayNameQuery,
        CancellationToken cancellationToken)
    {
        IQueryable<UserHealthCondition> query = _context.UserHealthConditions
            .AsNoTracking()
            .Include(x => x.HealthCondition)
            .Where(x => x.UserId == userId);

        query = ApplyDisplayNameFilter(query, displayNameQuery);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<UserHealthCondition?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.UserHealthConditions
            .Include(x => x.HealthCondition)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsDuplicateForUserAsync(
        Guid userId,
        string normalizedCustomName,
        CancellationToken cancellationToken)
    {
        return await _context.UserHealthConditions
            .AsNoTracking()
            .Include(x => x.HealthCondition)
            .AnyAsync(
                x => x.UserId == userId &&
                     (x.Source == HealthConditionSource.Custom &&
                      x.NormalizedCustomHealthConditionName != null &&
                      x.NormalizedCustomHealthConditionName == normalizedCustomName ||
                      x.Source == HealthConditionSource.Database &&
                      x.HealthCondition != null &&
                      x.HealthCondition.NormalizedHealthConditionName == normalizedCustomName),
                cancellationToken);
    }

    public async Task<bool> ExistsCatalogConditionForUserAsync(
        Guid userId,
        Guid healthConditionId,
        CancellationToken cancellationToken)
    {
        return await _context.UserHealthConditions
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId &&
                     x.Source == HealthConditionSource.Database &&
                     x.HealthConditionId == healthConditionId,
                cancellationToken);
    }

    public async Task AddAsync(UserHealthCondition userHealthCondition, CancellationToken cancellationToken)
    {
        await _context.UserHealthConditions.AddAsync(userHealthCondition, cancellationToken);
    }

    public void Remove(UserHealthCondition userHealthCondition)
    {
        _context.UserHealthConditions.Remove(userHealthCondition);
    }

    private static IQueryable<UserHealthCondition> ApplyDisplayNameFilter(
        IQueryable<UserHealthCondition> query,
        string? displayNameQuery)
    {
        if (string.IsNullOrWhiteSpace(displayNameQuery))
        {
            return query;
        }

        string trimmedQuery = displayNameQuery.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        return query.Where(userHealthCondition =>
            userHealthCondition.Source == HealthConditionSource.Database &&
            userHealthCondition.HealthCondition != null &&
            (EF.Functions.ILike(
                 userHealthCondition.HealthCondition.NormalizedHealthConditionName,
                 prefixPattern) ||
             EF.Functions.ILike(
                 userHealthCondition.HealthCondition.NormalizedHealthConditionName,
                 containsPattern) ||
             useTrigram &&
             EF.Functions.TrigramsSimilarity(
                 userHealthCondition.HealthCondition.NormalizedHealthConditionName,
                 trimmedQuery) >= SimilarityThreshold) ||
            userHealthCondition.Source == HealthConditionSource.Custom &&
            userHealthCondition.NormalizedCustomHealthConditionName != null &&
            (EF.Functions.ILike(
                 userHealthCondition.NormalizedCustomHealthConditionName,
                 prefixPattern) ||
             EF.Functions.ILike(
                 userHealthCondition.NormalizedCustomHealthConditionName,
                 containsPattern) ||
             useTrigram &&
             EF.Functions.TrigramsSimilarity(
                 userHealthCondition.NormalizedCustomHealthConditionName,
                 trimmedQuery) >= SimilarityThreshold));
    }

    private static IQueryable<UserHealthCondition> ApplySort(IQueryable<UserHealthCondition> query) =>
        query.OrderBy(userHealthCondition =>
                userHealthCondition.Source == HealthConditionSource.Database
                    ? userHealthCondition.HealthCondition!.NormalizedHealthConditionName
                    : userHealthCondition.NormalizedCustomHealthConditionName)
            .ThenBy(userHealthCondition => userHealthCondition.Id);
}
