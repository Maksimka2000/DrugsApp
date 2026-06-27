using DrugsApp.Features.AdminUserManagementFeature.ApplicationServices;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AdminUserManagementFeature.GetAdminUser;

/// <summary>
/// Request payload for retrieving an admin user by identifier.
/// </summary>
/// <param name="AdminUserId">Identifier of the admin user.</param>
public sealed record GetAdminUserRequest(Guid AdminUserId);

/// <summary>
/// Query to retrieve admin user details including current user context.
/// </summary>
/// <param name="AdminUserId">Identifier of the admin user to load.</param>
/// <param name="CurrentUserId">Identifier of the requesting admin.</param>
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
