using DrugsApp.Domain.AccountFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ClientUserManagementFeature.RemoveClientUser;

/// <summary>
/// Command to remove a client user.
/// </summary>
/// <param name="ClientUserId">Identifier of the client user.</param>
public sealed record RemoveClientUserCommand(Guid ClientUserId) : IRequest<ErrorOr<Success>>;

internal sealed class RemoveClientUserHandler(IClientAccountAdminDeletionService clientAccountAdminDeletionService)
    : IRequestHandler<RemoveClientUserCommand, ErrorOr<Success>>
{
    public Task<ErrorOr<Success>> Handle(
        RemoveClientUserCommand command,
        CancellationToken cancellationToken) =>
        clientAccountAdminDeletionService.DeleteByAdminAsync(command.ClientUserId, cancellationToken);
}
