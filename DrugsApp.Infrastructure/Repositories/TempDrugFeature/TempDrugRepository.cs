using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.TempDrugFeature;

public sealed class TempDrugRepository : ITempDrugRepository
{
    private const double SimilarityThreshold = 0.35;

    private readonly DrugsAppDbContext _context;

    public TempDrugRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    
    public async Task<TempDrug?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.TempDrugs
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByProductIdentifierAsync(string productIdentifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(productIdentifier))
        {
            return false;
        }

        string normalizedProductIdentifier = productIdentifier.Trim();
        return await _context.TempDrugs
            .AsNoTracking()
            .AnyAsync(tempDrug => tempDrug.ProductIdentifier == normalizedProductIdentifier, cancellationToken);
    }

    public async Task AddAsync(TempDrug tempDrug, CancellationToken cancellationToken)
    {
        await _context.TempDrugs.AddAsync(tempDrug, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        TempDrug? tempDrug = await _context.TempDrugs
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tempDrug is null)
        {
            return;
        }

        _context.TempDrugs.Remove(tempDrug);
    }

    public async Task<List<Guid>> GetIdsByImportBatchIdAsync(Guid importBatchId, CancellationToken cancellationToken)
    {
        return await _context.TempDrugs
            .AsNoTracking()
            .Where(tempDrug => tempDrug.LinkedImportId == importBatchId)
            .Select(tempDrug => tempDrug.Id)
            .ToListAsync(cancellationToken);
    }


    public async Task<List<TempDrug>> SearchAsync(
        string? searchQuery,
        IReadOnlyCollection<TempDrugStatus>? statusesFilter,
        IReadOnlyCollection<string>? typeOfProductsFilter,
        TempDrugSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<TempDrug> query = _context.TempDrugs.AsNoTracking();

        // apply filters
        query = ApplyStatusFilter(query, statusesFilter);
        query = ApplyTypeOfProductFilter(query, typeOfProductsFilter);
        
        // apply search filter
        query = ApplySearchFilter(query, searchQuery);

        // apply includes
        //

        // apply sort
        query = ApplySort(query, sortField, ascending);

        // apply paging
        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? searchQuery,
        IReadOnlyCollection<TempDrugStatus>? statusesFilter,
        IReadOnlyCollection<string>? typeOfProductsFilter,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<TempDrug> query = _context.TempDrugs.AsNoTracking();

        // apply filters
        query = ApplyStatusFilter(query, statusesFilter);
        query = ApplyTypeOfProductFilter(query, typeOfProductsFilter);

        // apply search filter
        query = ApplySearchFilter(query, searchQuery);

        // apply counting
        return await query.CountAsync(cancellationToken);
    }



    private static IQueryable<TempDrug> ApplyStatusFilter(
        IQueryable<TempDrug> query,
        IReadOnlyCollection<TempDrugStatus>? statusesFilter)
    {
        if (statusesFilter is null || statusesFilter.Count == 0)
        {
            return query;
        }

        return query.Where(tempDrug => statusesFilter.Contains(tempDrug.Status));
    }

    private static IQueryable<TempDrug> ApplyTypeOfProductFilter(
        IQueryable<TempDrug> query,
        IReadOnlyCollection<string>? typeOfProductsFilter)
    {
        if (typeOfProductsFilter is null || typeOfProductsFilter.Count == 0)
        {
            return query;
        }

        var filterList = typeOfProductsFilter.ToList();
        return query.Where(tempDrug => tempDrug.TypeOfProduct != null && filterList.Contains(tempDrug.TypeOfProduct));
    }

    private static IQueryable<TempDrug> ApplySearchFilter(
        IQueryable<TempDrug> query,
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

        return query.Where(tempDrug =>
             tempDrug.CommercialName != null && (EF.Functions.ILike(tempDrug.CommercialName, prefixPattern) ||
                                                 EF.Functions.ILike(tempDrug.CommercialName, containsPattern) ||
                                                 useTrigram &&
                                                 EF.Functions.TrigramsSimilarity(tempDrug.CommercialName, trimmedQuery) >= SimilarityThreshold));
    }

    private static IQueryable<TempDrug> ApplySort(
        IQueryable<TempDrug> query,
        TempDrugSortField sortField,
        bool ascending)
    {
        return (sortField, ascending) switch
        {
            (TempDrugSortField.CreatedAtUtc, true) => query
                .OrderBy(t => t.CreatedAtUtc)
                .ThenBy(t => t.Id),
            (TempDrugSortField.CreatedAtUtc, false) => query
                .OrderByDescending(t => t.CreatedAtUtc)
                .ThenBy(t => t.Id),

            (TempDrugSortField.UpdatedAtUtc, true) => query
                .OrderBy(t => t.UpdatedAtUtc)
                .ThenBy(t => t.Id),
            (TempDrugSortField.UpdatedAtUtc, false) => query
                .OrderByDescending(t => t.UpdatedAtUtc)
                .ThenBy(t => t.Id),

            (TempDrugSortField.ProductIdentifier, true) => query
                .OrderBy(t => t.ProductIdentifier)
                .ThenBy(t => t.Id),
            (TempDrugSortField.ProductIdentifier, false) => query
                .OrderByDescending(t => t.ProductIdentifier)
                .ThenBy(t => t.Id),

            (TempDrugSortField.CommercialName, true) => query
                .OrderBy(t => t.CommercialName)
                .ThenBy(t => t.Id),
            (TempDrugSortField.CommercialName, false) => query
                .OrderByDescending(t => t.CommercialName)
                .ThenBy(t => t.Id),

            _ => query.OrderByDescending(t => t.CreatedAtUtc).ThenBy(t => t.Id)
        };
    }
}


