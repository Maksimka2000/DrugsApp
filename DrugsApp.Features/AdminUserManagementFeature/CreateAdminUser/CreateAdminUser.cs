using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AdminUserManagementFeature.CreateAdminUser;

public sealed record CreateAdminUserCommand(CreateAdminUserRequest Request)
    : IRequest<ErrorOr<CreateAdminUserResponse>>;

internal sealed class CreateAdminUserHandler(IIdentityUserManagementService identityUserManagementService)
    : IRequestHandler<CreateAdminUserCommand, ErrorOr<CreateAdminUserResponse>>
{
    public async Task<ErrorOr<CreateAdminUserResponse>> Handle(
        CreateAdminUserCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateAdminUserResult> result = await identityUserManagementService.CreateAdminUserAsync(
            command.Request.FullName,
            command.Request.Email,
            command.Request.AdminRole,
            cancellationToken);

        return result.Match<ErrorOr<CreateAdminUserResponse>>(
            created => created.MapToResponse(),
            errors => errors);
    }
}
