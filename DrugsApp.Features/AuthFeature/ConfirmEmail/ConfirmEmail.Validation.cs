using FluentValidation;

namespace DrugsApp.Features.AuthFeature.ConfirmEmail;

public sealed class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(request => request.Token)
            .NotEmpty();
    }
}
