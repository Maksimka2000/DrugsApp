using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;

namespace DrugsApp.Domain.FoodFeature.Repositories;

/// <summary>
/// Persistence contract for querying and maintaining food aggregates.
/// </summary>
public interface IFoodRepository
{
    /// <summary>
    /// Searches foods by normalized search text with paging.
    /// </summary>
    /// <param name="searchText">Optional normalized search text.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of foods.</returns>
    Task<List<Food>> SearchAsync(
        string? searchText,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts foods matching the search text.
    /// </summary>
    /// <param name="searchText">Optional normalized search text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total count of matching foods.</returns>
    Task<int> CountAsync(
        string? searchText,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads a food aggregate by identifier.
    /// </summary>
    /// <param name="foodId">Unique food identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tracked food or null.</returns>
    Task<Food?> GetByIdAsync(Guid foodId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a food by canonical name using case-insensitive match.
    /// </summary>
    /// <param name="name">Canonical name to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching food or null.</returns>
    Task<Food?> GetByNameAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Loads multiple foods by identifiers.
    /// </summary>
    /// <param name="ids">Food identifiers to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of resolved foods.</returns>
    Task<List<Food>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether the food is referenced by related aggregates.
    /// </summary>
    /// <param name="foodId">Food identifier to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when references exist.</returns>
    Task<bool> IsReferencedAsync(Guid foodId, CancellationToken cancellationToken);

    /// <summary>
    /// Stages a new food aggregate for persistence.
    /// </summary>
    /// <param name="food">Food to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Asynchronous operation.</returns>
    Task AddAsync(Food food, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a food aggregate for deletion.
    /// </summary>
    /// <param name="food">Food to remove.</param>
    void Remove(Food food);

    /// <summary>
    /// Lists food group identifiers that contain the specified food.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetFoodGroupIdsContainingAsync(
        Guid foodId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the food groups that contain the specified food.
    /// </summary>
    Task<List<FoodGroup>> ListGroupsByFoodIdAsync(
        Guid foodId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists interaction identifiers where the food participates (party or expanded rows).
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetInteractionIdsReferencingAsync(
        Guid foodId,
        CancellationToken cancellationToken);
}
