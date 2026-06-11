using FluentValidation;

namespace DrugsApp.Features.AdminUserManagementFeature.GetAdminUser;

public sealed class GetAdminUserRequestValidator : AbstractValidator<GetAdminUserRequest>
{
    public GetAdminUserRequestValidator()
    {
        RuleFor(x => x.AdminUserId)
            .NotEmpty();
    }
}
