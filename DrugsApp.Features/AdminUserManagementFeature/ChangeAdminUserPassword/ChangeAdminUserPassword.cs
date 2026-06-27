using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AdminUserManagementFeature.ChangeAdminUserPassword;

/// <summary>
/// Command to change the current admin user's password.
/// </summary>
/// <param name="CurrentUserId">Identifier of the admin performing the change.</param>
/// <param name="Request">Payload containing the new password.</param>
public sealed record ChangeAdminUserPasswordCommand(
    Guid CurrentUserId,
    ChangeAdminUserPasswordRequest Request) : IRequest<ErrorOr<Success>>;

internal sealed class ChangeAdminUserPasswordHandler(IIdentityUserManagementService identityUserManagementService)
    : IRequestHandler<ChangeAdminUserPasswordCommand, ErrorOr<Success>>
{
    public Task<ErrorOr<Success>> Handle(
        ChangeAdminUserPasswordCommand command,
        CancellationToken cancellationToken) =>
        identityUserManagementService.SetAdminPasswordAsync(
            command.CurrentUserId,
            command.Request.NewPassword,
            cancellationToken);
}
