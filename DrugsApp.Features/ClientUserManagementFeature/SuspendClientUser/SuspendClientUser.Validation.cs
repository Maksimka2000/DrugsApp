using FluentValidation;

namespace DrugsApp.Features.ClientUserManagementFeature.SuspendClientUser;

public sealed record SuspendClientUserRouteRequest(Guid ClientUserId, bool IsSuspended);

public sealed class SuspendClientUserRouteRequestValidator : AbstractValidator<SuspendClientUserRouteRequest>
{
    public SuspendClientUserRouteRequestValidator()
    {
        RuleFor(x => x.ClientUserId)
            .NotEmpty();
    }
}
