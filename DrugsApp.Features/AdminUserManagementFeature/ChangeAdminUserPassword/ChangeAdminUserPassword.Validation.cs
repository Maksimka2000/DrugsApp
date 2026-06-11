using DrugsApp.Features.AdminUserManagementFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.AdminUserManagementFeature.ChangeAdminUserPassword;

public sealed class ChangeAdminUserPasswordRequestValidator : AbstractValidator<ChangeAdminUserPasswordRequest>
{
    public ChangeAdminUserPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match.");
    }
}
