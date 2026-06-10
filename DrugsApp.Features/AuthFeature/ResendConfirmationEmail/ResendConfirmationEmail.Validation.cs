using FluentValidation;

namespace DrugsApp.Features.AuthFeature.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailRequestValidator : AbstractValidator<ResendConfirmationEmailRequest>
{
    public ResendConfirmationEmailRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
