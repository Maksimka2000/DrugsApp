using System;
using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.CheckOtherAllergyNameAvailability;

public sealed class CheckOtherAllergyNameAvailabilityRequestValidator
    : AbstractValidator<CheckOtherAllergyNameAvailabilityRequest>
{
    public CheckOtherAllergyNameAvailabilityRequestValidator()
    {
        RuleFor(r => r.AllergyName).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(256);
        When(r => r.ExcludeId is not null, () => RuleFor(r => r.ExcludeId!.Value).NotEqual(Guid.Empty));
    }
}
