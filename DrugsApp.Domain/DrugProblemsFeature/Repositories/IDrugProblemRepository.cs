using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugProblemsFeature.Entities;

namespace DrugsApp.Domain.DrugProblemsFeature.Repositories;

/// <summary>
/// Persistence contract for user-owned drug problems.
/// </summary>
public interface IDrugProblemRepository
{
    Task<List<DrugProblem>> SearchByUserIdAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<DrugProblem?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        bool track,
        CancellationToken cancellationToken);

    Task AddAsync(DrugProblem drugProblem, CancellationToken cancellationToken);

    void Remove(DrugProblem drugProblem);

    Task<bool> ExistsForUserWithDrugIdAsync(
        Guid userId,
        Guid drugId,
        Guid? excludeId,
        CancellationToken cancellationToken);

    Task<bool> ExistsForUserWithCustomNameAsync(
        Guid userId,
        string normalizedCustomName,
        Guid? excludeId,
        CancellationToken cancellationToken);

    Task<bool> ExistsForUserWithAnyDrugDisplayNameAsync(
        Guid userId,
        string normalizedDrugDisplayName,
        Guid? excludeId,
        CancellationToken cancellationToken);

    Task<Guid?> FindConflictingCustomNameAsync(
        Guid userId,
        string normalizedCustomName,
        Guid? excludeId,
        CancellationToken cancellationToken);
}
