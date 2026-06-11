using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.AdminUserManagementFeature.UpdateAdminUserRole;

public sealed class UpdateAdminUserRoleRequestValidator : AbstractValidator<UpdateAdminUserRoleRequest>
{
    public UpdateAdminUserRoleRequestValidator()
    {
        RuleFor(x => x.AdminRole)
            .NotEmpty()
            .Must(role => AuthRoles.AdminRoleNames.Contains(role))
            .WithMessage("admin_role must be one of the allowed admin roles.");
    }
}
