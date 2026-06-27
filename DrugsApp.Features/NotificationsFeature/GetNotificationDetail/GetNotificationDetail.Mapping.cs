using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Features.NotificationsFeature.Shared;

namespace DrugsApp.Features.NotificationsFeature.GetNotificationDetail;

/// <summary>
/// Mapping helpers for building notification detail responses.
/// </summary>
public static class GetNotificationDetailMapping
{
    /// <summary>
    /// Maps a notification entity and payload to a detail response DTO.
    /// </summary>
    /// <param name="notification">Notification entity.</param>
    /// <param name="detail">Hydrated detail payload.</param>
    /// <returns>Detail response DTO.</returns>
    public static NotificationDetailResponse MapToDetailResponse(
        this UserNotification notification,
        NotificationDetailPayloadDto detail)
        => new(
            notification.Id,
            notification.TypeCode.ToString(),
            notification.PrimaryTagCode.ToString(),
            notification.AudienceScopeCode.ToString(),
            notification.Title,
            notification.SummaryText,
            notification.BodyText,
            notification.CreatedAtUtc,
            notification.IsRead,
            notification.RequiresAction,
            notification.ActionState.ToString(),
            notification.DeepLinkPath,
            notification.RelatedAggregateType,
            notification.RelatedAggregateId,
            detail);
}
