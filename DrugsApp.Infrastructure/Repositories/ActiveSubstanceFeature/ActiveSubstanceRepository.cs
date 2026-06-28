using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.ActiveSubstanceFeature;

public sealed class ActiveSubstanceRepository : IActiveSubstanceRepository
{
    private const double SimilarityThreshold = 0.35;
    
    private readonly DrugsAppDbContext _context;

    public ActiveSubstanceRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<ActiveSubstance?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.ActiveSubstances
            .Include(a => a.OralMedicationIntakeGuidelines)
            .Include(a => a.AlcoholInteractionOverride)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<ActiveSubstance?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        string trimmedName = name.Trim();

        return await _context.ActiveSubstances
            .AsNoTracking()
            .FirstOrDefaultAsync(a => EF.Functions.ILike(a.ActiveSubstanceName, trimmedName), cancellationToken);
    }

    public async Task AddAsync(ActiveSubstance activeSubstance, CancellationToken cancellationToken)
    {
        await _context.ActiveSubstances.AddAsync(activeSubstance, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ActiveSubstance>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<ActiveSubstance>();
        }

        return await _context.ActiveSubstances
            .AsNoTracking()
            .Include(activeSubstance => activeSubstance.OralMedicationIntakeGuidelines)
            .Include(activeSubstance => activeSubstance.AlcoholInteractionOverride)
            .Where(activeSubstance => ids.Contains(activeSubstance.Id))
            .OrderBy(activeSubstance => activeSubstance.ActiveSubstanceName)
            .ThenBy(activeSubstance => activeSubstance.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ActiveSubstance>> GetByIdsForLinkingAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<ActiveSubstance>();
        }

        return await _context.ActiveSubstances
            .Where(activeSubstance => ids.Contains(activeSubstance.Id))
            .OrderBy(activeSubstance => activeSubstance.ActiveSubstanceName)
            .ThenBy(activeSubstance => activeSubstance.Id)
            .ToListAsync(cancellationToken);
    }







    public async Task<List<ActiveSubstance>> SearchAsync(
        string? searchQuery,
        bool? alcohol,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<ActiveSubstance> query = _context.ActiveSubstances.AsNoTracking();

        // apply filters
        query = ApplyAlcoholFilter(query, alcohol);

        // apply search 
        query = ApplySearchFilter(query, searchQuery);

        // apply includes
        query = ApplyIncludes(query);

        // apply sort
        query = ApplySort(query);

        // apply paging
        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? searchQuery,
        bool? alcohol,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<ActiveSubstance> query = _context.ActiveSubstances.AsNoTracking();

        // apply filters
        query = ApplyAlcoholFilter(query, alcohol);

        // apply search filter
        query = ApplySearchFilter(query, searchQuery);

        // apply counting
        return await query.CountAsync(cancellationToken);
    }

    private static IQueryable<ActiveSubstance> ApplySearchFilter(
        IQueryable<ActiveSubstance> query,
        string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return query;
        }

        string trimmedQuery = searchQuery.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        return query.Where(a =>
            EF.Functions.ILike(a.ActiveSubstanceName, prefixPattern) ||
            EF.Functions.ILike(a.ActiveSubstanceName, containsPattern) ||
            useTrigram &&
            EF.Functions.TrigramsSimilarity(a.ActiveSubstanceName, trimmedQuery) >= SimilarityThreshold);

    }

    private static IQueryable<ActiveSubstance> ApplyIncludes(IQueryable<ActiveSubstance> query) =>
        query
            .Include(a => a.OralMedicationIntakeGuidelines)
            .Include(a => a.AlcoholInteractionOverride);

    private static IQueryable<ActiveSubstance> ApplySort(IQueryable<ActiveSubstance> query) =>
        query.OrderByDescending(a => a.ActiveSubstanceName);

    private static IQueryable<ActiveSubstance> ApplyAlcoholFilter(
        IQueryable<ActiveSubstance> query,
        bool? alcohol)
    {
        if (alcohol is not true)
        {
            return query;
        }

        return query.Where(a =>
            a.HasAlcoholInteraction ||
            a.AlcoholInteractionOverride != null);
    }

    public void Remove(ActiveSubstance activeSubstance)
    {
        _context.ActiveSubstances.Remove(activeSubstance);
    }

}


