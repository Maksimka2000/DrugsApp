using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AdminUserManagementFeature.RemoveAdminUser;

/// <summary>
/// Command to remove an admin user account.
/// </summary>
/// <param name="AdminUserId">Identifier of the admin user to remove.</param>
/// <param name="CallerUserId">Identifier of the admin performing the action.</param>
public sealed record RemoveAdminUserCommand(Guid AdminUserId, Guid CallerUserId)
    : IRequest<ErrorOr<Success>>;

internal sealed class RemoveAdminUserHandler(IIdentityUserManagementService identityUserManagementService)
    : IRequestHandler<RemoveAdminUserCommand, ErrorOr<Success>>
{
    public Task<ErrorOr<Success>> Handle(
        RemoveAdminUserCommand command,
        CancellationToken cancellationToken) =>
        identityUserManagementService.RemoveAdminUserAsync(
            command.AdminUserId,
            command.CallerUserId,
            cancellationToken);
}
