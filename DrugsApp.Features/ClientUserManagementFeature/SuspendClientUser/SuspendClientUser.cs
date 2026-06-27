using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.ClientUserManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ClientUserManagementFeature.SuspendClientUser;

/// <summary>
/// Command to update client user suspension state.
/// </summary>
/// <param name="ClientUserId">Identifier of the client user.</param>
/// <param name="IsSuspended">Desired suspension state.</param>
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
