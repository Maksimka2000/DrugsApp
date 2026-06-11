using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.AdminUserManagementFeature.ApplicationServices;

public interface IAdminUserManagementApplicationService
{
    Task<ErrorOr<ListAdminUsersResponse>> ListAsync(
        int skip,
        int take,
        string? search,
        Guid currentUserId,
        CancellationToken cancellationToken);

    Task<ErrorOr<AdminUserDetailDto>> GetByIdAsync(
        Guid adminUserId,
        Guid currentUserId,
        CancellationToken cancellationToken);
}
