using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularFoodFeature.Entities;

namespace DrugsApp.Domain.RegularFoodFeature.Interfaces;

/// <summary>
/// Persistence contract for user-owned regular food schedules.
/// </summary>
public interface IClientRegularFoodRepository
{
    /// <summary>
    /// Retrieves active regular food schedules for a user with optional filtering and paging.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="foodNameQuery">Optional case-insensitive name filter.</param>
    /// <param name="startDate">Optional lower bound for active period.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum number of rows to return.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Paged collection of active regular foods.</returns>
    Task<List<ClientRegularFood>> SearchLiveByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the catalog food identifiers of every regular food whose status is Active.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Distinct food identifiers without loading intake schedules.</returns>
    Task<List<Guid>> ListActiveFoodIdsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts active regular foods for a user respecting the same filters as the live search.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="foodNameQuery">Optional case-insensitive name filter.</param>
    /// <param name="startDate">Optional lower bound for active period.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Number of active rows matching the filter.</returns>
    Task<int> CountLiveByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves historical regular foods for a user with optional filtering and paging.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="foodNameQuery">Optional case-insensitive name filter.</param>
    /// <param name="startDate">Optional inclusive start of history window.</param>
    /// <param name="endDate">Optional inclusive end of history window.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum number of rows to return.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Paged collection of historical regular foods.</returns>
    Task<List<ClientRegularFood>> SearchHistoryByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts historical regular foods for a user respecting the same filters as the history search.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="foodNameQuery">Optional case-insensitive name filter.</param>
    /// <param name="startDate">Optional inclusive start of history window.</param>
    /// <param name="endDate">Optional inclusive end of history window.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Number of historical rows matching the filter.</returns>
    Task<int> CountHistoryByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a regular food record by identifier for the specified user.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="id">Regular food identifier.</param>
    /// <param name="track">Whether to track the entity in the context.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Regular food entity when found; otherwise null.</returns>
    Task<ClientRegularFood?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        bool track,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks if an active regular food exists for the user with the given catalog food identifier.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="foodId">Catalog food identifier.</param>
    /// <param name="excludeId">Optional entity identifier to exclude from the lookup.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns><c>true</c> when an active row exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsLiveForUserWithFoodIdAsync(
        Guid userId,
        Guid foodId,
        Guid? excludeId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new regular food record for a user.
    /// </summary>
    /// <param name="clientRegularFood">Entity to add.</param>
    /// <param name="cancellationToken">Propagation token to cancel the command.</param>
    Task AddAsync(ClientRegularFood clientRegularFood, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a regular food record for removal.
    /// </summary>
    /// <param name="clientRegularFood">Entity to remove.</param>
    void Remove(ClientRegularFood clientRegularFood);

    /// <summary>
    /// Removes expired regular food records for the specified user.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="cancellationToken">Propagation token to cancel the command.</param>
    /// <returns>Number of removed rows.</returns>
    Task<int> RemoveExpiredByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists active rows that have ended by the specified date for background cleanup.
    /// </summary>
    /// <param name="asOfDate">Reference date for end-of-period evaluation.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Collection of ended live rows eligible for archival.</returns>
    Task<List<ClientRegularFood>> ListEndedLiveRowsAsync(
        DateOnly asOfDate,
        int skip,
        int take,
        CancellationToken cancellationToken);
}
