using FluentValidation;

namespace DrugsApp.Features.ClientUserManagementFeature.ListClientUsers;

public sealed class ListClientUsersRequestValidator : AbstractValidator<ListClientUsersRequest>
{
    public ListClientUsersRequestValidator()
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
