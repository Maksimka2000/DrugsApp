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
    private readonly DrugsAppDbContext _context;

    public PharmaceuticalFormRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PharmaceuticalForm>> SearchAsync(
        string? nameFilter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<PharmaceuticalForm> query = BuildSearchQueryable(nameFilter);

        List<PharmaceuticalForm> result = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<int> CountAsync(
        string? nameFilter,
        CancellationToken cancellationToken)
    {
        IQueryable<PharmaceuticalForm> query = BuildSearchQueryable(nameFilter);

        int result = await query.CountAsync(cancellationToken);

        return result;
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
    private IQueryable<PharmaceuticalForm> BuildSearchQueryable(string? nameFilter)
    {
        string? trimmedFilter = nameFilter?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedFilter))
        {
            return _context.PharmaceuticalForms
                .AsNoTracking()
                .OrderBy(pf => pf.FormulationName);
        }

        if (trimmedFilter.Length < 3)
        {
            return _context.PharmaceuticalForms
                .AsNoTracking()
                .Where(pf => EF.Functions.ILike(pf.FormulationName, $"%{trimmedFilter}%"))
                .OrderBy(pf => pf.FormulationName);
        }

        return _context.PharmaceuticalForms
            .FromSqlInterpolated($"""
                SELECT * FROM "PharmaceuticalForms" pf
                WHERE pf."FormulationName" ILIKE '%' || {trimmedFilter} || '%'
                   OR pf."FormulationName" % {trimmedFilter}
                ORDER BY similarity(pf."FormulationName", {trimmedFilter}) DESC,
                         pf."FormulationName"
                """)
            .AsNoTracking();
    }
}


