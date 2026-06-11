using FluentValidation;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CreateProviderRequest;

public sealed class CreateProviderRequestRequestValidator : AbstractValidator<CreateProviderRequestRequest>
{
    public CreateProviderRequestRequestValidator()
    {
        RuleFor(x => x.PatientEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}
