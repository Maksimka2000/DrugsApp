using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Domain.DrugFeature.Repositories;

/// <summary>
/// Persistence contract for querying and loading drug aggregates.
/// </summary>
public interface IDrugRepository
{
    /// <summary>
    /// Searches drugs using the combined text filter, optional type filters, and paging metadata.
    /// </summary>
    /// <param name="searchQuery">Mixed-field search query (optional).</param>
    /// <param name="typeOfDrugsFilter">Optional type filter set.</param>
    /// <param name="searchField">Field used for executing the search (None when no query provided).</param>
    /// <param name="sortField">Sort field applied to the final projection.</param>
    /// <param name="ascending">Sort order flag.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of drugs (take + 1 results when computing has_more).</returns>
    Task<List<Drug>> SearchAsync(
        string? searchQuery,
        IReadOnlyCollection<DrugType>? typeOfDrugsFilter,
        DrugSearchField searchField,
        DrugSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the total number of records matching the same filters used for search queries.
    /// </summary>
    /// <param name="searchQuery">Mixed-field search query (optional).</param>
    /// <param name="typeOfDrugsFilter">Optional type filter set.</param>
    /// <param name="searchField">Field used for executing the search (None when no query provided).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total number of matching records.</returns>
    Task<int> CountAsync(
        string? searchQuery,
        IReadOnlyCollection<DrugType>? typeOfDrugsFilter,
        DrugSearchField searchField,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads a single drug aggregate including active substances for update workflows.
    /// </summary>
    /// <param name="drugId">Unique drug identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tracked drug aggregate or null.</returns>
    Task<Drug?> GetByIdAsync(Guid drugId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new drug aggregate to the repository.
    /// </summary>
    /// <param name="drug">Drug aggregate to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task AddAsync(Drug drug, CancellationToken cancellationToken);

    /// <summary>
    /// Searches for similar candidates based on commercial name and type of product.
    /// </summary>
    /// <param name="commercialName">Commercial name to search for.</param>
    /// <param name="typeOfProduct">Type of product to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of similar drug aggregates.</returns>
    Task<List<Drug>> SearchSimilarCandidatesAsync(
        string commercialName,
        string typeOfProduct,
        CancellationToken cancellationToken);
}

