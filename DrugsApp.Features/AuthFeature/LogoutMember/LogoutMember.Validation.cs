using FluentValidation;

namespace DrugsApp.Features.AuthFeature.LogoutMember;

public sealed class LogoutMemberRequestValidator : AbstractValidator<LogoutMemberRequest>
{
    public LogoutMemberRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty();
    }
}
