using System;
using DrugsApp.Domain.NotificationsFeature.Enums;

namespace DrugsApp.Domain.NotificationsFeature;

/// <summary>
/// Input payload for idempotent inbox projection writes from source features.
/// </summary>
public sealed record NotificationProjectionDraft(
    Guid UserId,
    Guid? SystemAnnouncementId,
    UserNotificationType TypeCode,
    NotificationTag PrimaryTagCode,
    NotificationAudienceScope AudienceScopeCode,
    string Title,
    string SummaryText,
    string? BodyText,
    string SourceEventKey,
    string? RelatedAggregateType,
    Guid? RelatedAggregateId,
    string? DeepLinkPath,
    bool RequiresAction,
    NotificationActionState? ActionState = null);
