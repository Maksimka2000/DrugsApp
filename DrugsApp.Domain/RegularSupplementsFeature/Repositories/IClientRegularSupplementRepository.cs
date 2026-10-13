using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;

namespace DrugsApp.Domain.RegularSupplementsFeature.Repositories;

/// <summary>
/// Persistence contract for user-owned regular supplement schedules.
/// </summary>
public interface IClientRegularSupplementRepository
{
    Task<List<ClientRegularSupplement>> SearchLiveByUserIdAsync(
        Guid userId,
        string? nameQuery,
        DateOnly? startDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves every Active regular supplement row (catalog and own) with its catalog supplement, own supplement members, and intake times.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Active rows only; Scheduled and Expired rows are excluded.</returns>
    Task<List<ClientRegularSupplement>> ListActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the catalog supplement identifiers of every Active regular supplement sourced from the database.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Distinct supplement identifiers; own (custom) supplements are excluded.</returns>
    Task<List<Guid>> ListActiveDatabaseSupplementIdsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<int> CountLiveByUserIdAsync(
        Guid userId,
        string? nameQuery,
        DateOnly? startDate,
        CancellationToken cancellationToken);

    Task<List<ClientRegularSupplement>> SearchHistoryByUserIdAsync(
        Guid userId,
        string? nameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountHistoryByUserIdAsync(
        Guid userId,
        string? nameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken);

    Task<ClientRegularSupplement?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);

    Task<ClientRegularSupplement?> GetTrackedByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExistsLiveDuplicateForUserAsync(
        Guid userId,
        RegularSupplementSource source,
        Guid? supplementId,
        Guid? ownSupplementId,
        Guid? excludeId,
        CancellationToken cancellationToken);

    Task AddAsync(ClientRegularSupplement clientRegularSupplement, CancellationToken cancellationToken);

    void Remove(ClientRegularSupplement clientRegularSupplement);

    Task<int> DeleteAllExpiredByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<List<ClientRegularSupplement>> FindEndedLiveRowsAsync(
        DateOnly asOfDate,
        int take,
        CancellationToken cancellationToken);
}
