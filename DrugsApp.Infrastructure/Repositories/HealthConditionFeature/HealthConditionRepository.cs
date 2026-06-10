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

public sealed class HealthConditionRepository : IHealthConditionRepository
{
    private const double SimilarityThreshold = 0.35;

    private readonly DrugsAppDbContext _context;

    public HealthConditionRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<HealthCondition>> SearchAsync(
        string? normalizedNameQuery,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<HealthCondition> query = _context.HealthConditions.AsNoTracking();

        query = ApplySearch(query, normalizedNameQuery);

        query = ApplySort(query);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? normalizedNameQuery,
        CancellationToken cancellationToken)
    {
        IQueryable<HealthCondition> query = _context.HealthConditions.AsNoTracking();

        query = ApplySearch(query, normalizedNameQuery);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<HealthCondition?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.HealthConditions
            .FirstOrDefaultAsync(healthCondition => healthCondition.Id == id, cancellationToken);
    }

    public async Task<HealthCondition?> GetByNormalizedNameAsync(
        string normalizedName,
        CancellationToken cancellationToken)
    {
        return await _context.HealthConditions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                healthCondition => healthCondition.NormalizedHealthConditionName == normalizedName,
                cancellationToken);
    }

    public async Task AddAsync(HealthCondition healthCondition, CancellationToken cancellationToken)
    {
        await _context.HealthConditions.AddAsync(healthCondition, cancellationToken);
    }

    public void Remove(HealthCondition healthCondition)
    {
        _context.HealthConditions.Remove(healthCondition);
    }

    private static IQueryable<HealthCondition> ApplySearch(
        IQueryable<HealthCondition> query,
        string? normalizedNameQuery)
    {
        if (string.IsNullOrWhiteSpace(normalizedNameQuery))
        {
            return query;
        }

        string trimmedQuery = normalizedNameQuery.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        return query.Where(healthCondition =>
            EF.Functions.ILike(healthCondition.NormalizedHealthConditionName, prefixPattern) ||
            EF.Functions.ILike(healthCondition.NormalizedHealthConditionName, containsPattern) ||
            useTrigram &&
            EF.Functions.TrigramsSimilarity(
                healthCondition.NormalizedHealthConditionName,
                trimmedQuery) >= SimilarityThreshold);
    }

    private static IQueryable<HealthCondition> ApplySort(IQueryable<HealthCondition> query) =>
        query.OrderBy(healthCondition => healthCondition.NormalizedHealthConditionName)
            .ThenBy(healthCondition => healthCondition.Id);
}
