using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;

namespace DrugsApp.Domain.AllergiesFeature.Repositories;

/// <summary>
/// Persistence contract for user-owned drug allergy records.
/// </summary>
public interface IClientDrugAllergyRepository
{
    Task<List<ClientDrugAllergy>> SearchAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(Guid userId, CancellationToken cancellationToken);

    Task<ClientDrugAllergy?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(ClientDrugAllergy allergy, CancellationToken cancellationToken);

    void Remove(ClientDrugAllergy allergy);

    Task<bool> ExistsByDrugIdAsync(
        Guid userId,
        Guid drugId,
        CancellationToken cancellationToken);

    Task<bool> ExistsCustomNameNormalizedAsync(
        Guid userId,
        string customDrugNameNormalized,
        CancellationToken cancellationToken);

    Task<bool> ExistsDatabaseDisplayNameNormalizedAsync(
        Guid userId,
        string drugDisplayNameNormalized,
        CancellationToken cancellationToken);

    Task<bool> HasActiveSubstanceOverlapAsync(
        Guid userId,
        Guid candidateDrugId,
        CancellationToken cancellationToken);
}
