using FluentValidation;

namespace DrugsApp.Features.AccountFeature.RequestLoginEmailChange;

public sealed class RequestLoginEmailChangeRequestValidator : AbstractValidator<RequestLoginEmailChangeRequest>
{
    public RequestLoginEmailChangeRequestValidator()
    {
        RuleFor(r => r.NewEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(r => r.CurrentPassword)
            .NotEmpty();
    }
}
