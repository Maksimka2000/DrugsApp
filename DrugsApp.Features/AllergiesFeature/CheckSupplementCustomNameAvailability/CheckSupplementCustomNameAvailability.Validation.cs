using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.CheckSupplementCustomNameAvailability;

public sealed class CheckSupplementCustomNameAvailabilityRequestValidator
    : AbstractValidator<CheckSupplementCustomNameAvailabilityRequest>
{
    public CheckSupplementCustomNameAvailabilityRequestValidator()
    {
        RuleFor(r => r.CustomSupplementName).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(256);
    }
}
