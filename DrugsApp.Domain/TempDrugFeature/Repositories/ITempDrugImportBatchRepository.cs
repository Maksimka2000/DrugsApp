using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Domain.TempDrugFeature.Repositories;

/// <summary>
/// Persistence contract for temporary drug import batches.
/// </summary>
public interface ITempDrugImportBatchRepository
{
    /// <summary>
    /// Stages a new import batch for persistence.
    /// </summary>
    Task AddAsync(TempDrugImportBatch batch, CancellationToken cancellationToken);

    /// <summary>
    /// Loads import batch by identifier.
    /// </summary>
    Task<TempDrugImportBatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Returns paged list of import batches ordered by creation timestamp.
    /// </summary>
    Task<List<TempDrugImportBatch>> SearchAsync(
        TempDrugImportBatchSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts all import batches.
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns currently active import/revert batch if any.
    /// </summary>
    Task<TempDrugImportBatch?> GetActiveImportAsync(CancellationToken cancellationToken);
}

