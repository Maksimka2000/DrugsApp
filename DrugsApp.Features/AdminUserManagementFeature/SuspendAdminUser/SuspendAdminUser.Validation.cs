using FluentValidation;

namespace DrugsApp.Features.AdminUserManagementFeature.SuspendAdminUser;

public sealed record SuspendAdminUserRouteRequest(Guid AdminUserId, bool IsSuspended);

public sealed class SuspendAdminUserRouteRequestValidator : AbstractValidator<SuspendAdminUserRouteRequest>
{
    public SuspendAdminUserRouteRequestValidator()
    {
        RuleFor(x => x.AdminUserId)
            .NotEmpty();
    }
}
