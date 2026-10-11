using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.StudyModeFeature.ApplicationServices;
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

/// <summary>
/// Removes the admin account and, in the same transaction, every private study clinic it created (public
/// clinics stay available to the other study mode admins).
/// </summary>
internal sealed class RemoveAdminUserHandler(
    IIdentityUserManagementService identityUserManagementService,
    IStudyAccountTeardownApplicationService studyAccountTeardownApplicationService)
    : IRequestHandler<RemoveAdminUserCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        RemoveAdminUserCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<Success> removeResult = await identityUserManagementService.RemoveAdminUserAsync(
            command.AdminUserId,
            command.CallerUserId,
            cancellationToken);
        if (removeResult.IsError)
        {
            return removeResult.Errors;
        }

        return await studyAccountTeardownApplicationService.DeletePrivateClinicsOfCreatorAsync(
            command.AdminUserId,
            cancellationToken);
    }
}
