using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.RemoveSupervisionConnection;

public sealed class RemoveSupervisionConnectionRequestValidator : AbstractValidator<RemoveSupervisionConnectionRequest>
{
    public RemoveSupervisionConnectionRequestValidator()
    {
        RuleFor(x => x.ConnectionId)
            .NotEmpty();
    }
}
