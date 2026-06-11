using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.GetSupervisionRequest;

public sealed class GetSupervisionRequestRequestValidator : AbstractValidator<GetSupervisionRequestRequest>
{
    public GetSupervisionRequestRequestValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
    }
}
