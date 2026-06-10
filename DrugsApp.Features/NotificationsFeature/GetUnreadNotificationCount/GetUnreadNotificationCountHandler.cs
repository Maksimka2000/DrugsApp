using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Repositories;
using DrugsApp.Features.NotificationsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.NotificationsFeature.GetUnreadNotificationCount;

internal sealed class GetUnreadNotificationCountHandler(IUserNotificationRepository _userNotificationRepository)
    : IRequestHandler<GetUnreadNotificationCountQuery, ErrorOr<UnreadNotificationCountResponse>>
{
    public async Task<ErrorOr<UnreadNotificationCountResponse>> Handle(
        GetUnreadNotificationCountQuery query,
        CancellationToken cancellationToken)
    {
        int unreadCount = await _userNotificationRepository.CountUnreadAsync(
            query.UserId,
            query.IncludeProviderAudience,
            cancellationToken);

        bool hasActionRequired = await _userNotificationRepository.ExistsUnreadActionRequiredAsync(
            query.UserId,
            query.IncludeProviderAudience,
            cancellationToken);

        return query.MapToResponse(unreadCount, hasActionRequired);
    }
}
