using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.ClientUserManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ClientUserManagementFeature.SuspendClientUser;

public sealed record SuspendClientUserCommand(
    Guid ClientUserId,
    bool IsSuspended) : IRequest<ErrorOr<ClientUserSuspensionResponse>>;

internal sealed class SuspendClientUserHandler(IIdentityUserManagementService identityUserManagementService)
    : IRequestHandler<SuspendClientUserCommand, ErrorOr<ClientUserSuspensionResponse>>
{
    public async Task<ErrorOr<ClientUserSuspensionResponse>> Handle(
        SuspendClientUserCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<UserSuspensionResult> result = await identityUserManagementService.SetClientSuspensionAsync(
            command.ClientUserId,
            command.IsSuspended,
            cancellationToken);

        return result.Match<ErrorOr<ClientUserSuspensionResponse>>(
            suspension => new ClientUserSuspensionResponse(suspension.Id, suspension.Status),
            errors => errors);
    }
}
