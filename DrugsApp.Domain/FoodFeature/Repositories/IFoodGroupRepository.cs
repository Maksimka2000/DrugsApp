using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;

namespace DrugsApp.Domain.FoodFeature.Repositories;

/// <summary>
/// Persistence contract for querying and maintaining food group aggregates.
/// </summary>
public interface IFoodGroupRepository
{
    /// <summary>
    /// Searches food groups by group name.
    /// </summary>
    /// <param name="searchQuery">Optional group name filter.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of food groups.</returns>
    Task<List<FoodGroup>> SearchAsyncByGroupName(
        string? searchQuery,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Searches food groups by member food name.
    /// </summary>
    /// <param name="foodNameQuery">Member food name filter.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of food groups.</returns>
    Task<List<FoodGroup>> SearchAsyncByMemberFoodName(
        string foodNameQuery,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts food groups matching the provided query when searching by group name.
    /// </summary>
    /// <param name="searchQuery">Optional group name filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total count of matching groups.</returns>
    Task<int> CountByGroupNameAsync(
        string? searchQuery,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts food groups when searching by member food name.
    /// </summary>
    /// <param name="foodNameQuery">Member food name filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total count of matching groups.</returns>
    Task<int> CountByMemberFoodNameAsync(
        string foodNameQuery,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads a food group aggregate by identifier.
    /// </summary>
    /// <param name="foodGroupId">Unique food group identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tracked food group or null.</returns>
    Task<FoodGroup?> GetByIdAsync(Guid foodGroupId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a food group by canonical name using case-insensitive match.
    /// </summary>
    /// <param name="name">Group name to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching group or null.</returns>
    Task<FoodGroup?> GetByNameAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new food group aggregate.
    /// </summary>
    /// <param name="foodGroup">Food group to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Asynchronous operation.</returns>
    Task AddAsync(FoodGroup foodGroup, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a food group for deletion.
    /// </summary>
    /// <param name="foodGroup">Food group to remove.</param>
    void Remove(FoodGroup foodGroup);

    /// <summary>
    /// Checks whether a group with the exact membership set already exists.
    /// </summary>
    /// <param name="foodIds">Food identifiers composing the membership.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when a group with identical members exists.</returns>
    Task<bool> ExistsWithMembersAsync(
        IReadOnlyCollection<Guid> foodIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether the food group is referenced by interaction aggregates.
    /// </summary>
    /// <param name="foodGroupId">Food group identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when references exist.</returns>
    Task<bool> IsReferencedByInteractionsAsync(Guid foodGroupId, CancellationToken cancellationToken);

    /// <summary>
    /// Bulk loads food groups by identifiers with member foods included.
    /// </summary>
    Task<List<FoodGroup>> GetByIdsAsync(
        IReadOnlyCollection<Guid> foodGroupIds,
        CancellationToken cancellationToken);
}
