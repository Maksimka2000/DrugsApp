using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.CheckDrugCustomNameAvailability;

/// <summary>
/// Validator for drug custom name availability checks.
/// </summary>
public sealed class CheckDrugCustomNameAvailabilityRequestValidator
    : AbstractValidator<CheckDrugCustomNameAvailabilityRequest>
{
    public CheckDrugCustomNameAvailabilityRequestValidator()
    {
        RuleFor(request => request.CustomDrugName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(256);
    }
}
