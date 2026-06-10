using FluentValidation;

namespace DrugsApp.Features.NotificationsFeature.ListNotifications;

public sealed class ListNotificationsRequestValidator : AbstractValidator<ListNotificationsRequest>
{
    public ListNotificationsRequestValidator()
    {
        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);
    }
}
