using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;

namespace DrugsApp.Domain.AllergiesFeature.Repositories;

/// <summary>
/// Persistence contract for user-owned food allergy records.
/// </summary>
public interface IClientFoodAllergyRepository
{
    Task<List<ClientFoodAllergy>> SearchAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(Guid userId, CancellationToken cancellationToken);

    Task<ClientFoodAllergy?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(ClientFoodAllergy allergy, CancellationToken cancellationToken);

    void Remove(ClientFoodAllergy allergy);

    Task<bool> ExistsByFoodIdAsync(
        Guid userId,
        Guid foodId,
        CancellationToken cancellationToken);

    Task<bool> ExistsCustomNameNormalizedAsync(
        Guid userId,
        string customFoodNameNormalized,
        CancellationToken cancellationToken);

    Task<bool> ExistsDatabaseDisplayNameNormalizedAsync(
        Guid userId,
        string foodDisplayNameNormalized,
        CancellationToken cancellationToken);
}
