using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;

namespace DrugsApp.Domain.AllergiesFeature.Repositories;

/// <summary>
/// Persistence contract for user-owned other allergy records.
/// </summary>
public interface IClientOtherAllergyRepository
{
    Task<List<ClientOtherAllergy>> SearchAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<List<ClientOtherAllergy>> ListAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<int> CountAsync(Guid userId, CancellationToken cancellationToken);

    Task<ClientOtherAllergy?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(ClientOtherAllergy allergy, CancellationToken cancellationToken);

    void Remove(ClientOtherAllergy allergy);

    Task<bool> ExistsNameNormalizedAsync(
        Guid userId,
        string allergyNameNormalized,
        Guid? excludeId,
        CancellationToken cancellationToken);
}
