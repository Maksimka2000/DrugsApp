using FluentValidation;

namespace DrugsApp.Features.AuthFeature.RefreshAccessToken;

public sealed class RefreshAccessTokenRequestValidator : AbstractValidator<RefreshAccessTokenRequest>
{
    public RefreshAccessTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty();
    }
}
