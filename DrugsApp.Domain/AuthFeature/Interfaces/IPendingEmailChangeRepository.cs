using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Entities;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Repository contract for pending login email change entities.
/// </summary>
public interface IPendingEmailChangeRepository
{
    /// <summary>
    /// Retrieves the active pending email change for a user, if any.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Pending email change or null when none is active.</returns>
    Task<PendingEmailChange?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves any pending email change for a user (active or inactive).
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Pending email change or null when missing.</returns>
    Task<PendingEmailChange?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Persists a new pending email change entity.
    /// </summary>
    /// <param name="pendingEmailChange">Entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(PendingEmailChange pendingEmailChange, CancellationToken cancellationToken);

    /// <summary>
    /// Removes all pending email changes for the given user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the provided pending email change entity.
    /// </summary>
    /// <param name="pendingEmailChange">Entity to remove.</param>
    void Remove(PendingEmailChange pendingEmailChange);
}
