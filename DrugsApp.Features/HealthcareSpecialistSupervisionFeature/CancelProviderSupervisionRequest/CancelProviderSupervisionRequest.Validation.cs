using FluentValidation;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CancelProviderSupervisionRequest;

public sealed class CancelProviderSupervisionRequestRequestValidator
    : AbstractValidator<CancelProviderSupervisionRequestRequest>
{
    public CancelProviderSupervisionRequestRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty();
    }
}
