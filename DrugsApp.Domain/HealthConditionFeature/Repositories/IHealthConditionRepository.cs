using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;

namespace DrugsApp.Domain.HealthConditionFeature.Repositories;

/// <summary>
/// Persistence contract for catalog health conditions.
/// </summary>
public interface IHealthConditionRepository
{
    Task<List<HealthCondition>> SearchAsync(
        string? normalizedNameQuery,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        string? normalizedNameQuery,
        CancellationToken cancellationToken);

    Task<HealthCondition?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<HealthCondition?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);

    Task AddAsync(HealthCondition healthCondition, CancellationToken cancellationToken);

    void Remove(HealthCondition healthCondition);
}
