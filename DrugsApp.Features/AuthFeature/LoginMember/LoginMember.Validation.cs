using FluentValidation;

namespace DrugsApp.Features.AuthFeature.LoginMember;

public sealed class LoginMemberRequestValidator : AbstractValidator<LoginMemberRequest>
{
    public LoginMemberRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(request => request.Password)
            .NotEmpty();
    }
}
