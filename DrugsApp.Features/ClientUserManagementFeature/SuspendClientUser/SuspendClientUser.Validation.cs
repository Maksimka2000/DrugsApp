using FluentValidation;

namespace DrugsApp.Features.ClientUserManagementFeature.SuspendClientUser;

/// <summary>
/// Route-level request combining client user id and suspension state.
/// </summary>
/// <param name="ClientUserId">Identifier of the client user.</param>
/// <param name="IsSuspended">Desired suspension state.</param>
public sealed record SuspendClientUserRouteRequest(Guid ClientUserId, bool IsSuspended);

public sealed class SuspendClientUserRouteRequestValidator : AbstractValidator<SuspendClientUserRouteRequest>
{
    public SuspendClientUserRouteRequestValidator()
    {
        RuleFor(x => x.ClientUserId)
            .NotEmpty();
    }
}
