using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.GetPatientHealthcareProfessional;

public sealed class GetPatientHealthcareProfessionalRequestValidator
    : AbstractValidator<GetPatientHealthcareProfessionalRequest>
{
    public GetPatientHealthcareProfessionalRequestValidator()
    {
        RuleFor(x => x.HealthcareProfessionalUserId)
            .NotEmpty();
    }
}
