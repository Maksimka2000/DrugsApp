using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AdminUserManagementFeature.SuspendAdminUser;

/// <summary>
/// Command to suspend or unsuspend an admin user.
/// </summary>
/// <param name="AdminUserId">Identifier of the admin being updated.</param>
/// <param name="IsSuspended">Flag indicating the desired suspension state.</param>
/// <param name="CallerUserId">Identifier of the admin performing the action.</param>
public sealed record SuspendAdminUserCommand(
    Guid AdminUserId,
    bool IsSuspended,
    Guid CallerUserId) : IRequest<ErrorOr<AdminUserSuspensionResponse>>;

internal sealed class SuspendAdminUserHandler(IIdentityUserManagementService identityUserManagementService)
    : IRequestHandler<SuspendAdminUserCommand, ErrorOr<AdminUserSuspensionResponse>>
{
    public async Task<ErrorOr<AdminUserSuspensionResponse>> Handle(
        SuspendAdminUserCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<UserSuspensionResult> result = await identityUserManagementService.SetAdminSuspensionAsync(
            command.AdminUserId,
            command.IsSuspended,
            command.CallerUserId,
            cancellationToken);

        return result.Match<ErrorOr<AdminUserSuspensionResponse>>(
            suspension => new AdminUserSuspensionResponse(suspension.Id, suspension.Status),
            errors => errors);
    }
}
