using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Features.NotificationsFeature.Shared;

namespace DrugsApp.Features.NotificationsFeature.GetNotificationDetail;

public static class GetNotificationDetailMapping
{
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
