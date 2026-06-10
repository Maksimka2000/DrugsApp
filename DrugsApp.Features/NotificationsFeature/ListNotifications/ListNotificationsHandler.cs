using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Domain.NotificationsFeature.Repositories;
using DrugsApp.Features.NotificationsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.NotificationsFeature.ListNotifications;

internal sealed class ListNotificationsHandler(IUserNotificationRepository _userNotificationRepository)
    : IRequestHandler<ListNotificationsQuery, ErrorOr<NotificationListResponse>>
{
    public async Task<ErrorOr<NotificationListResponse>> Handle(
        ListNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        List<UserNotification> items = await _userNotificationRepository.ListAsync(
            query.UserId,
            query.IncludeProviderAudience,
            query.Skip,
            query.Take,
            cancellationToken);

        int totalCount = await _userNotificationRepository.CountVisibleAsync(
            query.UserId,
            query.IncludeProviderAudience,
            cancellationToken);

        return items.MapToResponse(totalCount, query.Skip);
    }
}
