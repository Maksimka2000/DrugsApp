using FluentValidation;

namespace DrugsApp.Features.RegularSupplementsFeature.CheckOwnSupplementNameAvailability;

/// <summary>
/// Validates own supplement name availability request payloads.
/// </summary>
public sealed class CheckOwnSupplementNameAvailabilityRequestValidator
    : AbstractValidator<CheckOwnSupplementNameAvailabilityRequest>
{
    public CheckOwnSupplementNameAvailabilityRequestValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("name must not be empty.")
            .MaximumLength(200);
    }
}
