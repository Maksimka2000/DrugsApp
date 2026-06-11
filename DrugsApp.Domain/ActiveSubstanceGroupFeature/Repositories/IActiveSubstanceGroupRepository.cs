using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;

namespace DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;

/// <summary>
/// Repository contract for active substance group aggregates.
/// </summary>
public interface IActiveSubstanceGroupRepository
{
    /// <summary>
    /// Searches groups optionally filtered by active substance name.
    /// </summary>
    Task<List<ActiveSubstanceGroup>> SearchAsync(
        string? activeSubstanceName,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts groups using the same filter semantics as <see cref="SearchAsync"/>.
    /// </summary>
    Task<int> CountAsync(
        string? activeSubstanceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a group by identifier with tracking for updates.
    /// </summary>
    Task<ActiveSubstanceGroup?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a group by name for uniqueness checks.
    /// </summary>
    Task<ActiveSubstanceGroup?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new group for insertion.
    /// </summary>
    Task AddAsync(
        ActiveSubstanceGroup group,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a group for deletion.
    /// </summary>
    Task RemoveAsync(
        ActiveSubstanceGroup group,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any other group owns exactly the provided member set.
    /// </summary>
    Task<bool> ExistsWithExactMembersAsync(
        IReadOnlyCollection<Guid> memberIds,
        Guid? excludedGroupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists groups containing any of the provided active substance member ids.
    /// </summary>
    Task<List<ActiveSubstanceGroup>> ListByMemberIdsAsync(
        IReadOnlyCollection<Guid> memberIds,
        CancellationToken cancellationToken = default);
}
