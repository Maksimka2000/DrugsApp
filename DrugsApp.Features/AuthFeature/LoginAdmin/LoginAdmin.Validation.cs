using FluentValidation;

namespace DrugsApp.Features.AuthFeature.LoginAdmin;

public sealed class LoginAdminRequestValidator : AbstractValidator<LoginAdminRequest>
{
    public LoginAdminRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(request => request.Password)
            .NotEmpty();
    }
}
