using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AdminUserManagementFeature.Shared;

namespace DrugsApp.Features.AdminUserManagementFeature.CreateAdminUser;

internal static class CreateAdminUserMapping
{
    public static CreateAdminUserCommand MapToCommand(this CreateAdminUserRequest request) =>
        new(request);

    public static CreateAdminUserResponse MapToResponse(this CreateAdminUserResult result) =>
        new(
            Id: result.Id,
            FullName: result.FullName,
            Email: result.Email,
            AdminRole: result.AdminRole,
            Password: result.Password);
}
