using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.VetDrugFeature.Entities;
using DrugsApp.Domain.VetDrugFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DrugsApp.Domain.VetDrugFeature.Entities;

namespace DrugsApp.Infrastructure.Repositories.VetDrugFeature;

public sealed class VetDrugRepository : IVetDrugRepository
{
    private readonly DrugsAppDbContext _context;

    public VetDrugRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<VetDrug>> SearchAsync(
        string? nameFilter,
        bool isAscending,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<VetDrug> query = _context.VetDrugs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(vd => EF.Functions.ILike(vd.CommercialName, $"%{nameFilter}%"));
        }

        query = isAscending
            ? query.OrderBy(vd => vd.CommercialName)
            : query.OrderByDescending(vd => vd.CommercialName);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<VetDrug?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.VetDrugs
            .FirstOrDefaultAsync(vd => vd.Id == id, cancellationToken);
    }
}

