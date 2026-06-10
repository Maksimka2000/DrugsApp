using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Domain.NotificationsFeature.Repositories;
using DrugsApp.Features.NotificationsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.NotificationsFeature.MarkNotificationRead;

internal sealed class MarkNotificationReadHandler(IUserNotificationRepository _userNotificationRepository)
    : IRequestHandler<MarkNotificationReadCommand, ErrorOr<MarkNotificationReadResponse>>
{
    public async Task<ErrorOr<MarkNotificationReadResponse>> Handle(
        MarkNotificationReadCommand command,
        CancellationToken cancellationToken)
    {
        UserNotification? notification = await _userNotificationRepository.GetTrackedByIdForUserAsync(
            command.NotificationId,
            command.UserId,
            command.IncludeProviderAudience,
            cancellationToken);

        if (notification is null)
        {
            return Error.NotFound(
                code: "Notification.NotFound",
                description: "Notification was not found.",
                metadata: new()
                {
                    ["resourceType"] = "UserNotification",
                    ["resourceId"] = command.NotificationId.ToString()
                });
        }

        ErrorOr<Success> markResult = notification.MarkAsRead(DateTime.UtcNow);
        if (markResult.IsError)
        {
            return markResult.Errors;
        }

        return notification.MapToMarkReadResponse();
    }
}
