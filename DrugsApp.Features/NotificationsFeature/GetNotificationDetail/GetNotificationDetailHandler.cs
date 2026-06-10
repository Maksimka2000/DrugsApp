using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Domain.NotificationsFeature.Repositories;
using DrugsApp.Features.NotificationsFeature.ApplicationServices;
using DrugsApp.Features.NotificationsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.NotificationsFeature.GetNotificationDetail;

internal sealed class GetNotificationDetailHandler(
    IUserNotificationRepository _userNotificationRepository,
    INotificationDetailHydrationApplicationService _notificationDetailHydrationApplicationService)
    : IRequestHandler<GetNotificationDetailQuery, ErrorOr<NotificationDetailResponse>>
{
    public async Task<ErrorOr<NotificationDetailResponse>> Handle(
        GetNotificationDetailQuery query,
        CancellationToken cancellationToken)
    {
        UserNotification? notification = await _userNotificationRepository.GetByIdForUserAsync(
            query.NotificationId,
            query.UserId,
            query.IncludeProviderAudience,
            cancellationToken);

        if (notification is null)
        {
            return Error.NotFound(
                code: "Notification.NotFound",
                description: "Notification was not found.",
                metadata: new()
                {
                    ["resourceType"] = "UserNotification",
                    ["resourceId"] = query.NotificationId.ToString()
                });
        }

        ErrorOr<NotificationDetailPayloadDto> detail = await _notificationDetailHydrationApplicationService
            .BuildDetailAsync(notification, query.UserId, cancellationToken);
        if (detail.IsError)
        {
            return detail.Errors;
        }

        return notification.MapToDetailResponse(detail.Value);
    }
}
