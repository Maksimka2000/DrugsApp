using FluentValidation;

namespace DrugsApp.Features.AccountFeature.ConfirmLoginEmailChange;

public sealed class ConfirmLoginEmailChangeRequestValidator : AbstractValidator<ConfirmLoginEmailChangeRequest>
{
    public ConfirmLoginEmailChangeRequestValidator()
    {
        RuleFor(r => r.UserId)
            .NotEmpty();

        RuleFor(r => r.Token)
            .NotEmpty();
    }
}
