using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.RejectSupervisionRequest;

public sealed class RejectSupervisionRequestRequestValidator : AbstractValidator<RejectSupervisionRequestRequest>
{
    public RejectSupervisionRequestRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty();
    }
}
