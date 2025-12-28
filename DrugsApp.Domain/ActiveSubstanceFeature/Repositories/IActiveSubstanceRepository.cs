using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;

namespace DrugsApp.Domain.ActiveSubstanceFeature.Repositories;

public interface IActiveSubstanceRepository
{
    Task<List<ActiveSubstance>> SearchAsync(
        string? nameFilter,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        string? nameFilter,
        CancellationToken cancellationToken);

    Task<ActiveSubstance?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ActiveSubstance?> GetByNameAsync(string name, CancellationToken cancellationToken);

    Task AddAsync(ActiveSubstance activeSubstance, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ActiveSubstance>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);
}


