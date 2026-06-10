using FluentValidation;

namespace DrugsApp.Features.AccountFeature.ChangeAccountPassword;

public sealed class ChangeAccountPasswordRequestValidator : AbstractValidator<ChangeAccountPasswordRequest>
{
    public ChangeAccountPasswordRequestValidator()
    {
        RuleFor(r => r.CurrentPassword)
            .NotEmpty();

        RuleFor(r => r.NewPassword)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(r => r.ConfirmNewPassword)
            .Equal(r => r.NewPassword)
            .WithMessage("Passwords do not match.");

        RuleFor(r => r)
            .Must(r => r.NewPassword != r.CurrentPassword)
            .WithMessage("New password must differ from current password.");
    }
}
