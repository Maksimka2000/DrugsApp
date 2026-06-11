using DrugsApp.Features.AdminUserManagementFeature.ApplicationServices;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AdminUserManagementFeature.GetAdminUser;

public sealed record GetAdminUserRequest(Guid AdminUserId);

public sealed record GetAdminUserQuery(Guid AdminUserId, Guid CurrentUserId)
    : IRequest<ErrorOr<AdminUserDetailDto>>;

internal sealed class GetAdminUserHandler(IAdminUserManagementApplicationService adminUserManagementApplicationService)
    : IRequestHandler<GetAdminUserQuery, ErrorOr<AdminUserDetailDto>>
{
    public Task<ErrorOr<AdminUserDetailDto>> Handle(
        GetAdminUserQuery query,
        CancellationToken cancellationToken) =>
        adminUserManagementApplicationService.GetByIdAsync(
            query.AdminUserId,
            query.CurrentUserId,
            cancellationToken);
}
