using DrugsApp.Features.MealTimeManagementFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.MealTimeManagementFeature.CreateMealTime;

/// <summary>
/// Validates create meal time requests.
/// </summary>
public sealed class CreateMealTimeRequestValidator : AbstractValidator<CreateMealTimeRequest>
{
    public CreateMealTimeRequestValidator()
    {
        RuleFor(request => request.MealName)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Meal name is required.")
            .MaximumLength(128)
            .WithMessage("Meal name must not exceed 128 characters.");

        RuleFor(request => request.IntakeTime)
            .Must(value => MealTimeMappings.TryParseIntakeTime(value, out _))
            .WithMessage("Intake time must be a valid HH:mm value.");
    }
}
