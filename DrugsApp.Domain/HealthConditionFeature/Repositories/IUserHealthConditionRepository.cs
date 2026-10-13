using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;

namespace DrugsApp.Domain.HealthConditionFeature.Repositories;

/// <summary>
/// Persistence contract for user health condition records.
/// </summary>
public interface IUserHealthConditionRepository
{
    Task<List<UserHealthCondition>> SearchAsync(
        Guid userId,
        string? displayNameQuery,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        Guid userId,
        string? displayNameQuery,
        CancellationToken cancellationToken);

    Task<List<UserHealthCondition>> ListAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserHealthCondition?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsDuplicateForUserAsync(Guid userId, string normalizedCustomName, CancellationToken cancellationToken);

    Task<bool> ExistsCatalogConditionForUserAsync(Guid userId, Guid healthConditionId, CancellationToken cancellationToken);

    Task AddAsync(UserHealthCondition userHealthCondition, CancellationToken cancellationToken);

    void Remove(UserHealthCondition userHealthCondition);
}
