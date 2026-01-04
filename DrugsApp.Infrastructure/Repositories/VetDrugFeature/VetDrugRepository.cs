using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.VetDrugFeature.Entities;
using DrugsApp.Domain.VetDrugFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.VetDrugFeature;

public sealed class VetDrugRepository(DrugsAppDbContext context) : IVetDrugRepository
{
    private readonly DrugsAppDbContext _context = context;

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

    public async Task<VetDrug?> GetByIdAsync(Guid id, CancellationToken cancellationToken) {
        return await _context.VetDrugs.FirstOrDefaultAsync(vd => vd.Id == id, cancellationToken);
    } 

    public async Task<VetDrug?> GetByCommercialNameAsync(string commercialName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(commercialName))
        {
            return null;
        }

        string trimmedName = commercialName.Trim();

        return await _context.VetDrugs
            .AsNoTracking()
            .FirstOrDefaultAsync(vd => EF.Functions.ILike(vd.CommercialName, trimmedName), cancellationToken);
    }

    public async Task AddAsync(VetDrug vetDrug, CancellationToken cancellationToken)
    {
        await _context.VetDrugs.AddAsync(vetDrug, cancellationToken);
    }
}

