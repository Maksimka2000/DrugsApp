using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature;
using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Domain.NotificationsFeature.Enums;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.NotificationsFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.NotificationsFeature.ApplicationServices;

public sealed class NotificationProjectionApplicationService(
    IUserNotificationRepository _userNotificationRepository) : INotificationProjectionService
{
    public async Task<ErrorOr<Success>> ProjectAsync(
        NotificationProjectionDraft draft,
        CancellationToken cancellationToken)
    {
        string normalizedSourceEventKey = draft.SourceEventKey.Trim();
        if (string.IsNullOrWhiteSpace(normalizedSourceEventKey))
        {
            return Error.Validation(
                code: "NotificationProjection.SourceEventKeyRequired",
                description: "Source event key must be provided.");
        }

        UserNotification? existing = await _userNotificationRepository.GetTrackedBySourceEventKeyAsync(
            draft.UserId,
            normalizedSourceEventKey,
            cancellationToken);

        NotificationProjectionDraft normalizedDraft = draft with
        {
            SourceEventKey = normalizedSourceEventKey,
            Title = draft.Title.Trim(),
            SummaryText = draft.SummaryText.Trim(),
            BodyText = string.IsNullOrWhiteSpace(draft.BodyText) ? null : draft.BodyText.Trim(),
            RelatedAggregateType = string.IsNullOrWhiteSpace(draft.RelatedAggregateType)
                ? null
                : draft.RelatedAggregateType.Trim(),
            DeepLinkPath = string.IsNullOrWhiteSpace(draft.DeepLinkPath) ? null : draft.DeepLinkPath.Trim()
        };

        if (existing is null)
        {
            ErrorOr<UserNotification> created = UserNotification.Create(normalizedDraft);
            if (created.IsError)
            {
                return created.Errors;
            }

            await _userNotificationRepository.AddAsync(created.Value, cancellationToken);
            return Result.Success;
        }

        NotificationActionState actionState = normalizedDraft.ActionState
            ?? (normalizedDraft.RequiresAction
                ? NotificationActionState.Pending
                : NotificationActionState.Informational);

        ErrorOr<Success> updated = existing.UpdateProjection(
            normalizedDraft.Title,
            normalizedDraft.SummaryText,
            normalizedDraft.BodyText,
            normalizedDraft.DeepLinkPath,
            normalizedDraft.RequiresAction,
            actionState,
            normalizedDraft.RelatedAggregateType,
            normalizedDraft.RelatedAggregateId);

        return updated;
    }

    public async Task<ErrorOr<Success>> SyncActionStateAsync(
        Guid userId,
        string sourceEventKey,
        NotificationActionState actionState,
        bool requiresAction,
        CancellationToken cancellationToken)
    {
        string normalizedSourceEventKey = sourceEventKey.Trim();
        UserNotification? existing = await _userNotificationRepository.GetTrackedBySourceEventKeyAsync(
            userId,
            normalizedSourceEventKey,
            cancellationToken);

        if (existing is null)
        {
            // Inbox row may be absent (e.g. legacy seed data or projection skipped on create).
            // Sync is best-effort; missing projection is already the desired end state.
            return Result.Success;
        }

        return existing.SyncActionState(actionState, requiresAction);
    }
}
