using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.TempDrugFeature;

public sealed class TempDrugImportBatchRepository(DrugsAppDbContext context) : ITempDrugImportBatchRepository
{
    private readonly DrugsAppDbContext _context = context;

    public async Task AddAsync(TempDrugImportBatch batch, CancellationToken cancellationToken)
    {
        await _context.TempDrugImportBatches.AddAsync(batch, cancellationToken);
    }

    public async Task<TempDrugImportBatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.TempDrugImportBatches
            .FirstOrDefaultAsync(batch => batch.Id == id, cancellationToken);
    }

    public async Task<List<TempDrugImportBatch>> SearchAsync(
        TempDrugImportBatchSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<TempDrugImportBatch> query = _context.TempDrugImportBatches.AsNoTracking();

        // apply sort
        query = ApplySort(query, sortField, ascending);
        
        // apply paging
        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    

    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        // build query
        IQueryable<TempDrugImportBatch> query = _context.TempDrugImportBatches.AsNoTracking();

        // apply counting
        return await query.CountAsync(cancellationToken);
    }



    private static IQueryable<TempDrugImportBatch> ApplySort(
        IQueryable<TempDrugImportBatch> query,
        TempDrugImportBatchSortField sortField,
        bool ascending)
    {
        return (sortField, ascending) switch
        {
            (TempDrugImportBatchSortField.CreatedAtUtc, true) => query.OrderBy(batch => batch.CreatedAtUtc).ThenBy(batch => batch.Id),
            (TempDrugImportBatchSortField.CreatedAtUtc, false) => query.OrderByDescending(batch => batch.CreatedAtUtc).ThenBy(batch => batch.Id),
        };
    }

    public async Task<TempDrugImportBatch?> GetActiveImportAsync(CancellationToken cancellationToken)
    {
        return await _context.TempDrugImportBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(
                batch => batch.StatusOfImport == ImportBatchStatus.InProgress
                      || batch.StatusOfImport == ImportBatchStatus.RevertInProgress,
                cancellationToken);
    }


    
}

