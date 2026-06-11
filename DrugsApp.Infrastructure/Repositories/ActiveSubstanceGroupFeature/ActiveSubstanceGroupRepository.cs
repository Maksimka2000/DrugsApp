using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.ActiveSubstanceGroupFeature;

public sealed class ActiveSubstanceGroupRepository(DrugsAppDbContext _context) : IActiveSubstanceGroupRepository
{
    private const double SimilarityThreshold = 0.35;

    public async Task<List<ActiveSubstanceGroup>> SearchAsync(
        string? activeSubstanceName,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<ActiveSubstanceGroup> query = _context.ActiveSubstanceGroups
            .AsNoTracking()
            .Include(group => group.ActiveSubstances);

        // apply filters
        // apply search

        if (string.IsNullOrWhiteSpace(activeSubstanceName))
        {
            return await query
                .OrderBy(group => group.Name)
                .ThenBy(group => group.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        string trimmedQuery = activeSubstanceName.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        IQueryable<ActiveSubstanceGroup> groupNameQuery = query.Where(group =>
            EF.Functions.ILike(group.Name, prefixPattern) ||
            EF.Functions.ILike(group.Name, containsPattern) ||
            useTrigram &&
            EF.Functions.TrigramsSimilarity(group.Name, trimmedQuery) >= SimilarityThreshold);

        List<ActiveSubstanceGroup> nameResults = await groupNameQuery
            .OrderBy(group => group.Name)
            .ThenBy(group => group.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);




        // If there are results or we are skipping, return the results
        if (nameResults.Count > 0 || skip > 0)
        {
            return nameResults;
        }



        // If there are no results and we are not skipping, search by member name
        IQueryable<ActiveSubstanceGroup> membershipQuery = query.Where(group =>
            group.ActiveSubstances.Any(member =>
                EF.Functions.ILike(member.ActiveSubstanceName, prefixPattern) ||
                EF.Functions.ILike(member.ActiveSubstanceName, containsPattern) ||
                useTrigram &&
                EF.Functions.TrigramsSimilarity(member.ActiveSubstanceName, trimmedQuery) >= SimilarityThreshold));

        return await membershipQuery
            .OrderBy(group => group.Name)
            .ThenBy(group => group.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? activeSubstanceName,
        CancellationToken cancellationToken)
    {
        IQueryable<ActiveSubstanceGroup> query = _context.ActiveSubstanceGroups.AsNoTracking();

        if (string.IsNullOrWhiteSpace(activeSubstanceName))
        {
            return await query.CountAsync(cancellationToken);
        }

        string trimmedQuery = activeSubstanceName.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        int nameCount = await query.CountAsync(group =>
            EF.Functions.ILike(group.Name, prefixPattern) ||
            EF.Functions.ILike(group.Name, containsPattern) ||
            useTrigram &&
            EF.Functions.TrigramsSimilarity(group.Name, trimmedQuery) >= SimilarityThreshold,
            cancellationToken);

        if (nameCount > 0)
        {
            return nameCount;
        }

        return await query.CountAsync(group =>
            group.ActiveSubstances.Any(member =>
                EF.Functions.ILike(member.ActiveSubstanceName, prefixPattern) ||
                EF.Functions.ILike(member.ActiveSubstanceName, containsPattern) ||
                useTrigram &&
                EF.Functions.TrigramsSimilarity(member.ActiveSubstanceName, trimmedQuery) >= SimilarityThreshold),
            cancellationToken);
    }

    public async Task<ActiveSubstanceGroup?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.ActiveSubstanceGroups
            .Include(group => group.ActiveSubstances)
            .FirstOrDefaultAsync(group => group.Id == id, cancellationToken);
    }

    public async Task<ActiveSubstanceGroup?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken)
    {
        string trimmedName = name.Trim();

        return await _context.ActiveSubstanceGroups
            .AsNoTracking()
            .Include(group => group.ActiveSubstances)
            .FirstOrDefaultAsync(group => EF.Functions.ILike(group.Name, trimmedName), cancellationToken);
    }

    public async Task AddAsync(
        ActiveSubstanceGroup group,
        CancellationToken cancellationToken)
    {
        await _context.ActiveSubstanceGroups.AddAsync(group, cancellationToken);
    }

    public Task RemoveAsync(
        ActiveSubstanceGroup group,
        CancellationToken cancellationToken)
    {
        _context.ActiveSubstanceGroups.Remove(group);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsWithExactMembersAsync(
        IReadOnlyCollection<Guid> memberIds,
        Guid? excludedGroupId,
        CancellationToken cancellationToken)
    {
        var normalizedIds = memberIds.Distinct().ToList();

        if (normalizedIds.Count == 0)
        {
            return false;
        }

        IQueryable<ActiveSubstanceGroup> query = _context.ActiveSubstanceGroups.AsNoTracking();

        if (excludedGroupId.HasValue)
        {
            query = query.Where(group => group.Id != excludedGroupId.Value);
        }

        return await query
            .Where(group => group.ActiveSubstances.Count == normalizedIds.Count)
            .Where(group => group.ActiveSubstances.All(member => normalizedIds.Contains(member.Id)))
            .AnyAsync(cancellationToken);
    }

    public async Task<List<ActiveSubstanceGroup>> ListByMemberIdsAsync(
        IReadOnlyCollection<Guid> memberIds,
        CancellationToken cancellationToken)
    {
        if (memberIds.Count == 0)
        {
            return new List<ActiveSubstanceGroup>();
        }

        return await _context.ActiveSubstanceGroups
            .AsNoTracking()
            .Include(group => group.ActiveSubstances)
            .Where(group => group.ActiveSubstances.Any(member => memberIds.Contains(member.Id)))
            .OrderBy(group => group.Name)
            .ThenBy(group => group.Id)
            .ToListAsync(cancellationToken);
    }
}
