using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AdminUserManagementFeature.ApplicationServices;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AdminUserManagementFeature.ChangeAdminUserLoginEmail;

/// <summary>
/// Command to change the current admin user's login email.
/// </summary>
/// <param name="CurrentUserId">Identifier of the admin performing the change.</param>
/// <param name="Request">Payload containing the new email.</param>
public sealed record ChangeAdminUserLoginEmailCommand(
    Guid CurrentUserId,
    ChangeAdminUserLoginEmailRequest Request) : IRequest<ErrorOr<AdminUserDetailDto>>;

internal sealed class ChangeAdminUserLoginEmailHandler(
    IIdentityUserManagementService identityUserManagementService,
    IAdminUserManagementApplicationService adminUserManagementApplicationService)
    : IRequestHandler<ChangeAdminUserLoginEmailCommand, ErrorOr<AdminUserDetailDto>>
{
    public async Task<ErrorOr<AdminUserDetailDto>> Handle(
        ChangeAdminUserLoginEmailCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<AdminUserMutationResult> changeResult = await identityUserManagementService.ChangeAdminLoginEmailAsync(
            command.CurrentUserId,
            command.Request.Email,
            cancellationToken);

        if (changeResult.IsError)
        {
            return changeResult.Errors;
        }

        return await adminUserManagementApplicationService.GetByIdAsync(
            command.CurrentUserId,
            command.CurrentUserId,
            cancellationToken);
    }
}
