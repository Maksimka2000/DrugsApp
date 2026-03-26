using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.PharmaceuticalFormFeature;

public sealed class PharmaceuticalFormRepository : IPharmaceuticalFormRepository
{
    private const double SimilarityThreshold = 0.35;
    
    private readonly DrugsAppDbContext _context;

    public PharmaceuticalFormRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    

    public async Task<PharmaceuticalForm?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.PharmaceuticalForms
            .FirstOrDefaultAsync(pf => pf.Id == id, cancellationToken);
    }

    public async Task<PharmaceuticalForm?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _context.PharmaceuticalForms
            .AsNoTracking()
            .FirstOrDefaultAsync(pf => EF.Functions.ILike(pf.FormulationName, name), cancellationToken);
    }

    public async Task AddAsync(PharmaceuticalForm pharmaceuticalForm, CancellationToken cancellationToken)
    {
        await _context.PharmaceuticalForms.AddAsync(pharmaceuticalForm, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PharmaceuticalForm>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<PharmaceuticalForm>();
        }

        return await _context.PharmaceuticalForms
            .Where(pharmaceuticalForm => ids.Contains(pharmaceuticalForm.Id))
            .ToListAsync(cancellationToken);
    }







    public async Task<List<PharmaceuticalForm>> SearchAsync(
        string? searchQuery,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<PharmaceuticalForm> query = _context.PharmaceuticalForms.AsNoTracking();

        // apply filters

        // apply search filter
        query = ApplySearchFilter(query, searchQuery);

        // apply includes

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
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<PharmaceuticalForm> query = _context.PharmaceuticalForms.AsNoTracking();

        // apply filters

        // apply search filter
        query = ApplySearchFilter(query, searchQuery);

        // apply counting
        return await query.CountAsync(cancellationToken);
    }


    private static IQueryable<PharmaceuticalForm> ApplySearchFilter(
        IQueryable<PharmaceuticalForm> query,
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

        return query.Where(pf =>
            EF.Functions.ILike(pf.FormulationName, prefixPattern) ||
            EF.Functions.ILike(pf.FormulationName, containsPattern) ||
            useTrigram && EF.Functions.TrigramsSimilarity(pf.FormulationName, trimmedQuery) >= SimilarityThreshold);
    }

    private static IQueryable<PharmaceuticalForm> ApplySort(IQueryable<PharmaceuticalForm> query) =>
        query.OrderByDescending(pf => pf.FormulationName);
}


