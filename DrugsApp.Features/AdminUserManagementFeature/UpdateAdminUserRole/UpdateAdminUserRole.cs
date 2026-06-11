using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AdminUserManagementFeature.ApplicationServices;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AdminUserManagementFeature.UpdateAdminUserRole;

public sealed record UpdateAdminUserRoleCommand(
    Guid AdminUserId,
    Guid CurrentUserId,
    UpdateAdminUserRoleRequest Request) : IRequest<ErrorOr<AdminUserDetailDto>>;

internal sealed class UpdateAdminUserRoleHandler(
    IIdentityUserManagementService identityUserManagementService,
    IAdminUserManagementApplicationService adminUserManagementApplicationService)
    : IRequestHandler<UpdateAdminUserRoleCommand, ErrorOr<AdminUserDetailDto>>
{
    public async Task<ErrorOr<AdminUserDetailDto>> Handle(
        UpdateAdminUserRoleCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<AdminUserMutationResult> replaceResult = await identityUserManagementService.ReplaceAdminRoleAsync(
            command.AdminUserId,
            command.Request.AdminRole,
            command.CurrentUserId,
            cancellationToken);

        if (replaceResult.IsError)
        {
            return replaceResult.Errors;
        }

        return await adminUserManagementApplicationService.GetByIdAsync(
            command.AdminUserId,
            command.CurrentUserId,
            cancellationToken);
    }
}
