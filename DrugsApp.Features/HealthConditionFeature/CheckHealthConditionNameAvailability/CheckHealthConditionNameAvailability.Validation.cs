using System;
using FluentValidation;

namespace DrugsApp.Features.HealthConditionFeature.CheckHealthConditionNameAvailability;

/// <summary>
/// Validator for catalog health condition name availability request.
/// </summary>
public sealed class CheckHealthConditionNameAvailabilityRequestValidator
    : AbstractValidator<CheckHealthConditionNameAvailabilityRequest>
{
    public CheckHealthConditionNameAvailabilityRequestValidator()
    {
        RuleFor(request => request.HealthConditionName)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("health_condition_name must not be empty.")
            .MaximumLength(256);

        When(request => request.ExcludeId is not null, () =>
        {
            RuleFor(request => request.ExcludeId)
                .Must(id => id != Guid.Empty)
                .WithMessage("exclude_id must not be an empty Guid.");
        });
    }
}
