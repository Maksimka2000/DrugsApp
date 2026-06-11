using DrugsApp.Features.AdminUserManagementFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.AdminUserManagementFeature.ChangeAdminUserLoginEmail;

public sealed class ChangeAdminUserLoginEmailRequestValidator : AbstractValidator<ChangeAdminUserLoginEmailRequest>
{
    public ChangeAdminUserLoginEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}
