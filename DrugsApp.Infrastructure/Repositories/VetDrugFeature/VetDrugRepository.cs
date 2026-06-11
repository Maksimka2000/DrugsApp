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
    private const double SimilarityThreshold = 0.35;

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

    public async Task<List<VetDrug>> SearchByCommercialNameAsync(
        string searchQuery,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        string trimmedQuery = searchQuery.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        IQueryable<VetDrug> query = _context.VetDrugs
            .AsNoTracking()
            .Where(vetDrug =>
                EF.Functions.ILike(vetDrug.CommercialName, prefixPattern) ||
                EF.Functions.ILike(vetDrug.CommercialName, containsPattern) ||
                useTrigram &&
                EF.Functions.TrigramsSimilarity(vetDrug.CommercialName, trimmedQuery) >= SimilarityThreshold);

        query = query
            .OrderByDescending(vetDrug => EF.Functions.ILike(vetDrug.CommercialName, prefixPattern))
            .ThenByDescending(vetDrug => EF.Functions.ILike(vetDrug.CommercialName, containsPattern))
            .ThenByDescending(vetDrug =>
                useTrigram
                    ? EF.Functions.TrigramsSimilarity(vetDrug.CommercialName, trimmedQuery)
                    : 0)
            .ThenBy(vetDrug => vetDrug.CommercialName)
            .ThenBy(vetDrug => vetDrug.Id);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByCommercialNameAsync(
        string searchQuery,
        CancellationToken cancellationToken)
    {
        string trimmedQuery = searchQuery.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        return await _context.VetDrugs
            .AsNoTracking()
            .Where(vetDrug =>
                EF.Functions.ILike(vetDrug.CommercialName, prefixPattern) ||
                EF.Functions.ILike(vetDrug.CommercialName, containsPattern) ||
                useTrigram &&
                EF.Functions.TrigramsSimilarity(vetDrug.CommercialName, trimmedQuery) >= SimilarityThreshold)
            .CountAsync(cancellationToken);
    }
}

