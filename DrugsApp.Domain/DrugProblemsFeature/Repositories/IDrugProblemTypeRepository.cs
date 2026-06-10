using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugProblemsFeature.Entities;

namespace DrugsApp.Domain.DrugProblemsFeature.Repositories;

/// <summary>
/// Persistence contract for admin-managed drug problem types.
/// </summary>
public interface IDrugProblemTypeRepository
{
    Task<List<DrugProblemType>> SearchAsync(
        string? problemTypeNameQuery,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(string? problemTypeNameQuery, CancellationToken cancellationToken);

    Task<DrugProblemType?> GetByIdAsync(
        Guid id,
        bool track,
        CancellationToken cancellationToken);

    Task<DrugProblemType?> GetByNameAsync(string normalizedProblemTypeName, CancellationToken cancellationToken);

    Task AddAsync(DrugProblemType drugProblemType, CancellationToken cancellationToken);

    void Remove(DrugProblemType drugProblemType);
}
