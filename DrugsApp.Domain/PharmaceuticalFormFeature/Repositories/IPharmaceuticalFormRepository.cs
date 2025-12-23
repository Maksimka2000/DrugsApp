using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;

namespace DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;

public interface IPharmaceuticalFormRepository
{
    Task<List<PharmaceuticalForm>> SearchAsync(
        string? nameFilter,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        string? nameFilter,
        CancellationToken cancellationToken);

    Task<PharmaceuticalForm?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PharmaceuticalForm?> GetByNameAsync(string name, CancellationToken cancellationToken);

    Task AddAsync(PharmaceuticalForm pharmaceuticalForm, CancellationToken cancellationToken);
}


