using DrugsApp.Domain.SupplementFeature.Entities;

namespace DrugsApp.Domain.SupplementFeature.Repositories;

public interface ISupplementRepository
{
    Task<List<Supplement>> SearchAsync(
        string? nameFilter,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        string? nameFilter,
        CancellationToken cancellationToken);

    Task<Supplement?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Supplement?> GetByNameAsync(
        string normalizedName,
        CancellationToken cancellationToken);

    Task<List<Supplement>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads supplements by id with EF tracking so they can be linked to aggregates without duplicate inserts.
    /// </summary>
    Task<List<Supplement>> GetByIdsForLinkingAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);

    Task AddAsync(
        Supplement supplement,
        CancellationToken cancellationToken);

    void Remove(Supplement supplement);

    Task<bool> IsInAnyGroupAsync(
        Guid supplementId,
        CancellationToken cancellationToken);
}
