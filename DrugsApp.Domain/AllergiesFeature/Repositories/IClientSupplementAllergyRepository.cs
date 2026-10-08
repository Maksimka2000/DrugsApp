using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;

namespace DrugsApp.Domain.AllergiesFeature.Repositories;

/// <summary>
/// Persistence contract for user-owned supplement allergy records.
/// </summary>
public interface IClientSupplementAllergyRepository
{
    Task<List<ClientSupplementAllergy>> SearchAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<List<ClientSupplementAllergy>> ListAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<int> CountAsync(Guid userId, CancellationToken cancellationToken);

    Task<ClientSupplementAllergy?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(ClientSupplementAllergy allergy, CancellationToken cancellationToken);

    void Remove(ClientSupplementAllergy allergy);

    Task<bool> ExistsBySupplementIdAsync(
        Guid userId,
        Guid supplementId,
        CancellationToken cancellationToken);

    Task<bool> ExistsCustomNameNormalizedAsync(
        Guid userId,
        string customSupplementNameNormalized,
        CancellationToken cancellationToken);

    Task<bool> ExistsDatabaseDisplayNameNormalizedAsync(
        Guid userId,
        string supplementDisplayNameNormalized,
        CancellationToken cancellationToken);
}
