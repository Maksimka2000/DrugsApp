using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.CheckFoodCustomNameAvailability;

public sealed class CheckFoodCustomNameAvailabilityRequestValidator
    : AbstractValidator<CheckFoodCustomNameAvailabilityRequest>
{
    public CheckFoodCustomNameAvailabilityRequestValidator()
    {
        RuleFor(r => r.CustomFoodName).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(256);
    }
}
