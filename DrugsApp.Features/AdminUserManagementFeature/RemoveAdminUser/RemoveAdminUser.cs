using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AdminUserManagementFeature.RemoveAdminUser;

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
