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
    private readonly DrugsAppDbContext _context;

    public ActiveSubstanceRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActiveSubstance>> SearchAsync(
        string? nameFilter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ActiveSubstance> query = BuildSearchQueryable(nameFilter, includeGuidelines: true);

        List<ActiveSubstance> result = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<int> CountAsync(
        string? nameFilter,
        CancellationToken cancellationToken)
    {
        IQueryable<ActiveSubstance> query = BuildSearchQueryable( nameFilter, includeGuidelines: false);

        int result = await query.CountAsync(cancellationToken);

        return result;
    }

    public async Task<ActiveSubstance?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.ActiveSubstances
            .Include(a => a.OralMedicationIntakeGuidelines)
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
            .Where(activeSubstance => ids.Contains(activeSubstance.Id))
            .ToListAsync(cancellationToken);
    }
    private IQueryable<ActiveSubstance> BuildSearchQueryable(
        string? nameFilter,
        bool includeGuidelines)
    {
        string? trimmedFilter = nameFilter?.Trim();

        IQueryable<ActiveSubstance> query;

        if (string.IsNullOrWhiteSpace(trimmedFilter))
        {
            query = _context.ActiveSubstances
                .AsNoTracking()
                .OrderBy(a => a.ActiveSubstanceName);
        }
        else if (trimmedFilter.Length < 3)
        {
            query = _context.ActiveSubstances
                .AsNoTracking()
                .Where(a => EF.Functions.ILike(a.ActiveSubstanceName, $"%{trimmedFilter}%"))
                .OrderBy(a => a.ActiveSubstanceName);
        }
        else
        {
            query = _context.ActiveSubstances
                .FromSqlInterpolated($"""
                    SELECT * FROM "ActiveSubstances" AS a
                    WHERE a."ActiveSubstanceName" ILIKE '%' || {trimmedFilter} || '%'
                       OR a."ActiveSubstanceName" % {trimmedFilter}
                    ORDER BY similarity(a."ActiveSubstanceName", {trimmedFilter}) DESC,
                             a."ActiveSubstanceName"
                    """)
                .AsNoTracking();
        }

        if (includeGuidelines)
        {
            query = query.Include(a => a.OralMedicationIntakeGuidelines);
        }

        return query;
    }
}


