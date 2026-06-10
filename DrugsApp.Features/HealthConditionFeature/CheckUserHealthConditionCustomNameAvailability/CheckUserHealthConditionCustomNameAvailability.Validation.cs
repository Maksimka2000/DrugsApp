using FluentValidation;

namespace DrugsApp.Features.HealthConditionFeature.CheckUserHealthConditionCustomNameAvailability;

/// <summary>
/// Validator for checking custom health condition name availability.
/// </summary>
public sealed class CheckUserHealthConditionCustomNameAvailabilityRequestValidator
    : AbstractValidator<CheckUserHealthConditionCustomNameAvailabilityRequest>
{
    public CheckUserHealthConditionCustomNameAvailabilityRequestValidator()
    {
        RuleFor(request => request.CustomHealthConditionName)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("custom_health_condition_name must not be empty.")
            .MaximumLength(256);
    }
}
