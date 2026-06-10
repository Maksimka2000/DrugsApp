using System;
using FluentValidation;

namespace DrugsApp.Features.MealTimeManagementFeature.CheckMealTimeAvailability;

/// <summary>
/// Validates meal time availability check requests.
/// </summary>
public sealed class CheckMealTimeAvailabilityRequestValidator
    : AbstractValidator<CheckMealTimeAvailabilityRequest>
{
    public CheckMealTimeAvailabilityRequestValidator()
    {
        When(request => !string.IsNullOrWhiteSpace(request.MealName), () =>
        {
            RuleFor(request => request.MealName)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Meal name must not be whitespace.")
                .MaximumLength(128)
                .WithMessage("Meal name must not exceed 128 characters.");
        });

        When(request => request.ExcludeId is not null, () =>
        {
            RuleFor(request => request.ExcludeId)
                .Must(id => id != Guid.Empty)
                .WithMessage("Exclude id must not be an empty Guid.");
        });
    }
}
