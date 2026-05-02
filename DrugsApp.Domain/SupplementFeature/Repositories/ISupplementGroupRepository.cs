using DrugsApp.Domain.SupplementFeature.Entities;

namespace DrugsApp.Domain.SupplementFeature.Repositories;

public interface ISupplementGroupRepository
{
    Task<List<SupplementGroup>> SearchAsync(
        string? supplementNameQuery,
        SupplementGroupSearchField searchField,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        string? supplementNameQuery,
        SupplementGroupSearchField searchField,
        CancellationToken cancellationToken);

    Task<SupplementGroup?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<SupplementGroup?> GetByNameAsync(
        string normalizedName,
        CancellationToken cancellationToken);

    Task AddAsync(
        SupplementGroup group,
        CancellationToken cancellationToken);

    void Remove(SupplementGroup group);

    Task<bool> IsInUseByInteractionsAsync(
        Guid supplementGroupId,
        CancellationToken cancellationToken);
}
