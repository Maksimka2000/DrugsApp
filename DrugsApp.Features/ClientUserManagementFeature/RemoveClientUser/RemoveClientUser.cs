using DrugsApp.Domain.AccountFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ClientUserManagementFeature.RemoveClientUser;

public sealed record RemoveClientUserCommand(Guid ClientUserId) : IRequest<ErrorOr<Success>>;

internal sealed class RemoveClientUserHandler(IClientAccountAdminDeletionService clientAccountAdminDeletionService)
    : IRequestHandler<RemoveClientUserCommand, ErrorOr<Success>>
{
    public Task<ErrorOr<Success>> Handle(
        RemoveClientUserCommand command,
        CancellationToken cancellationToken) =>
        clientAccountAdminDeletionService.DeleteByAdminAsync(command.ClientUserId, cancellationToken);
}
