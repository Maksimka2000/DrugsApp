using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;

namespace DrugsApp.Domain.RegularSupplementsFeature.Repositories;

/// <summary>
/// Persistence contract for user-owned own supplement compounds.
/// </summary>
public interface IClientOwnSupplementRepository
{
    Task<List<ClientOwnSupplement>> ListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<ClientOwnSupplement?> GetByIdForUserWithMembersAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);

    Task<ClientOwnSupplement?> GetTrackedByIdForUserWithMembersAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);

    Task<ClientOwnSupplement?> FindByNormalizedNameForUserAsync(
        Guid userId,
        string normalizedName,
        Guid? excludeId,
        CancellationToken cancellationToken);

    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<(int LiveCount, int ExpiredCount)> CountReferencingRegularSupplementsAsync(
        Guid ownSupplementId,
        CancellationToken cancellationToken);

    Task AddAsync(ClientOwnSupplement clientOwnSupplement, CancellationToken cancellationToken);

    void Remove(ClientOwnSupplement clientOwnSupplement);
}
