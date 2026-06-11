using FluentValidation;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CancelProviderRequest;

public sealed class CancelProviderRequestRequestValidator : AbstractValidator<CancelProviderRequestRequest>
{
    public CancelProviderRequestRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty();
    }
}
