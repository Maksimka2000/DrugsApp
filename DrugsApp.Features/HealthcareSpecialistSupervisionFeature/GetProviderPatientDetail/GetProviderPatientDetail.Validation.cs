using FluentValidation;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.GetProviderPatientDetail;

public sealed class GetProviderPatientDetailRequestValidator : AbstractValidator<GetProviderPatientDetailRequest>
{
    public GetProviderPatientDetailRequestValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty();
    }
}
