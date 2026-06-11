using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.AdminUserManagementFeature.CreateAdminUser;

public sealed class CreateAdminUserRequestValidator : AbstractValidator<CreateAdminUserRequest>
{
    public CreateAdminUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.AdminRole)
            .NotEmpty()
            .Must(role => AuthRoles.AdminRoleNames.Contains(role))
            .WithMessage("admin_role must be one of the allowed admin roles.");
    }
}
