using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Domain.DrugFeature.Repositories;

/// <summary>
/// Persistence contract dedicated to drug form lookups for update workflows.
/// </summary>
public interface IDrugFormRepository
{
    /// <summary>
    /// Loads a drug form with all required navigation properties for update scenarios.
    /// </summary>
    /// <param name="drugFormId">Drug form identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tracked drug form aggregate or null.</returns>
    Task<DrugForm?> GetByIdAsync(Guid drugFormId, CancellationToken cancellationToken);

    /// <summary>
    /// Searches catalog drug forms using the combined text filter and paging metadata.
    /// </summary>
    /// <param name="searchQuery">Mixed-field search query (optional).</param>
    /// <param name="searchField">Field used for executing the search (None when no query provided).</param>
    /// <param name="sortField">Sort field applied when no search query is provided.</param>
    /// <param name="ascending">Sort order flag.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of drug forms with parent drug includes (take + 1 results when computing has_more).</returns>
    Task<List<DrugForm>> SearchFormsAsync(
        string? searchQuery,
        DrugFormSearchField searchField,
        DrugSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the total number of catalog drug forms matching the same filters used for search queries.
    /// </summary>
    /// <param name="searchQuery">Mixed-field search query (optional).</param>
    /// <param name="searchField">Field used for executing the search (None when no query provided).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total number of matching drug forms.</returns>
    Task<int> CountFormsAsync(
        string? searchQuery,
        DrugFormSearchField searchField,
        CancellationToken cancellationToken);
}

