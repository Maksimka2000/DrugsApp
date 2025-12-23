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
        IQueryable<PharmaceuticalForm> query = _context.PharmaceuticalForms.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(pf => EF.Functions.ILike(pf.FormulationName, $"%{nameFilter}%"));
        }

        query = query.OrderBy(pf => pf.FormulationName);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? nameFilter,
        CancellationToken cancellationToken)
    {
        IQueryable<PharmaceuticalForm> query = _context.PharmaceuticalForms.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(pf => EF.Functions.ILike(pf.FormulationName, $"%{nameFilter}%"));
        }

        return await query.CountAsync(cancellationToken);
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
}


