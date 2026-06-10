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
    Task<ErrorOr<Success>> ProjectAsync(
        NotificationProjectionDraft draft,
        CancellationToken cancellationToken);

    Task<ErrorOr<Success>> SyncActionStateAsync(
        Guid userId,
        string sourceEventKey,
        NotificationActionState actionState,
        bool requiresAction,
        CancellationToken cancellationToken);
}
