using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Domain.TempDrugFeature.Repositories;

/// <summary>
/// Persistence contract for querying and loading temporary drug aggregates.
/// </summary>
public interface ITempDrugRepository
{
    /// <summary>
    /// Searches temporary drugs using optional filters with stable sorting and pagination.
    /// </summary>
    /// <param name="commercialName">Optional search term applied to commercial name.</param>
    /// <param name="statusesFilter">Optional statuses filter set.</param>
    /// <param name="typeOfProductsFilter">Optional type-of-product filter set.</param>
    /// <param name="sortField">Sort field applied to the final projection.</param>
    /// <param name="ascending">Sort order flag.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Page of temporary drugs (caller may request take + 1 to compute has_more).</returns>
    Task<List<TempDrug>> SearchAsync(
        string? commercialName,
        IReadOnlyCollection<TempDrugStatus>? statusesFilter,
        IReadOnlyCollection<string>? typeOfProductsFilter,
        TempDrugSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts temporary drugs matching the same filters used for search queries.
    /// </summary>
    /// <param name="commercialName">Optional search term applied to commercial name.</param>
    /// <param name="statusesFilter">Optional statuses filter set.</param>
    /// <param name="typeOfProductsFilter">Optional type-of-product filter set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total number of matching records.</returns>
    Task<int> CountAsync(
        string? commercialName,
        IReadOnlyCollection<TempDrugStatus>? statusesFilter,
        IReadOnlyCollection<string>? typeOfProductsFilter,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads a single temporary drug aggregate by id for update workflows.
    /// </summary>
    /// <param name="id">Unique temporary drug identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tracked temporary drug aggregate or null.</returns>
    Task<TempDrug?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}


