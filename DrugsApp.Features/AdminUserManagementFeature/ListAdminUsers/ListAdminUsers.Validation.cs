using FluentValidation;

namespace DrugsApp.Features.AdminUserManagementFeature.ListAdminUsers;

public sealed class ListAdminUsersRequestValidator : AbstractValidator<ListAdminUsersRequest>
{
    public ListAdminUsersRequestValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 40);

        When(x => !string.IsNullOrWhiteSpace(x.Search), () =>
        {
            RuleFor(x => x.Search)
                .MaximumLength(200);
        });
    }
}
