using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.NotificationsFeature.Interfaces;

/// <summary>
/// Idempotent inbox projection contract consumed by source features and background jobs.
/// </summary>
public interface INotificationProjectionService
{
    /// <summary>
    /// Persists or updates a notification projection instance based on the supplied draft payload.
    /// </summary>
    /// <param name="draft">Draft snapshot describing the notification to materialize.</param>
    /// <param name="cancellationToken">Propagation token to cancel the projection operation.</param>
    /// <returns>Success when the projection is stored; an error when persistence fails.</returns>
    Task<ErrorOr<Success>> ProjectAsync(
        NotificationProjectionDraft draft,
        CancellationToken cancellationToken);

    /// <summary>
    /// Synchronizes the action state for an existing notification projection.
    /// </summary>
    /// <param name="userId">User owner of the notification projection.</param>
    /// <param name="sourceEventKey">Unique source event identifier used as a projection key.</param>
    /// <param name="actionState">New action or resolution state to apply.</param>
    /// <param name="requiresAction">Indicates whether the notification still requires user action.</param>
    /// <param name="cancellationToken">Propagation token to cancel the update.</param>
    /// <returns>Success when the state is updated; an error otherwise.</returns>
    Task<ErrorOr<Success>> SyncActionStateAsync(
        Guid userId,
        string sourceEventKey,
        NotificationActionState actionState,
        bool requiresAction,
        CancellationToken cancellationToken);
}
