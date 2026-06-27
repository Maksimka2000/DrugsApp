using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularDrugsFeature.Entities;

namespace DrugsApp.Domain.RegularDrugsFeature.Interfaces;

/// <summary>
/// Persistence contract for user-owned regular drug schedules.
/// </summary>
public interface IClientRegularDrugRepository
{
    /// <summary>
    /// Retrieves active regular drugs for a user with optional filtering and paging.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="drugNameQuery">Optional case-insensitive name filter.</param>
    /// <param name="startDate">Optional lower bound for active period.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum number of rows to return.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Paged collection of active regular drugs.</returns>
    Task<List<ClientRegularDrug>> SearchLiveByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts active regular drugs for a user respecting the same filters as the live search.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="drugNameQuery">Optional case-insensitive name filter.</param>
    /// <param name="startDate">Optional lower bound for active period.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Number of active rows matching the filter.</returns>
    Task<int> CountLiveByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves historical regular drugs for a user with optional filtering and paging.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="drugNameQuery">Optional case-insensitive name filter.</param>
    /// <param name="startDate">Optional inclusive start of history window.</param>
    /// <param name="endDate">Optional inclusive end of history window.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum number of rows to return.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Paged collection of historical regular drugs.</returns>
    Task<List<ClientRegularDrug>> SearchHistoryByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts historical regular drugs for a user respecting the same filters as the history search.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="drugNameQuery">Optional case-insensitive name filter.</param>
    /// <param name="startDate">Optional inclusive start of history window.</param>
    /// <param name="endDate">Optional inclusive end of history window.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Number of historical rows matching the filter.</returns>
    Task<int> CountHistoryByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a regular drug by identifier for the specified user.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="id">Regular drug identifier.</param>
    /// <param name="track">Whether to track the entity in the context.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Regular drug entity when found; otherwise null.</returns>
    Task<ClientRegularDrug?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        bool track,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks if an active regular drug exists for the user with the given catalog drug identifier.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="drugId">Catalog drug identifier.</param>
    /// <param name="excludeId">Optional entity identifier to exclude from the lookup.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns><c>true</c> when an active row exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsLiveForUserWithDrugIdAsync(
        Guid userId,
        Guid drugId,
        Guid? excludeId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new regular drug record for a user.
    /// </summary>
    /// <param name="clientRegularDrug">Entity to add.</param>
    /// <param name="cancellationToken">Propagation token to cancel the command.</param>
    Task AddAsync(ClientRegularDrug clientRegularDrug, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a regular drug record for removal.
    /// </summary>
    /// <param name="clientRegularDrug">Entity to remove.</param>
    void Remove(ClientRegularDrug clientRegularDrug);

    /// <summary>
    /// Deletes expired regular drug records for the specified user.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="cancellationToken">Propagation token to cancel the command.</param>
    /// <returns>Number of deleted rows.</returns>
    Task<int> DeleteExpiredByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Finds active rows that have ended by the specified date for background cleanup.
    /// </summary>
    /// <param name="asOfDate">Reference date for end-of-period evaluation.</param>
    /// <param name="take">Maximum rows to return.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Collection of ended live rows eligible for archival.</returns>
    Task<List<ClientRegularDrug>> FindEndedLiveRowsAsync(
        DateOnly asOfDate,
        int take,
        CancellationToken cancellationToken);
}
