using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AdminUserManagementFeature.SuspendAdminUser;

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
