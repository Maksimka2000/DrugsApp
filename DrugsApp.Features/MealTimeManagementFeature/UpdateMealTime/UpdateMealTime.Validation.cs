using FluentValidation;

namespace DrugsApp.Features.MealTimeManagementFeature.UpdateMealTime;

/// <summary>
/// Validates update meal time requests.
/// </summary>
public sealed class UpdateMealTimeRequestValidator : AbstractValidator<UpdateMealTimeRequest>
{
    public UpdateMealTimeRequestValidator()
    {
        RuleFor(request => request)
            .Must(request =>
                !string.IsNullOrWhiteSpace(request.MealName) ||
                request.IntakeTime is not null)
            .WithMessage("At least one of mealName or intakeTime must be provided.");

        When(request => !string.IsNullOrWhiteSpace(request.MealName), () =>
        {
            RuleFor(request => request.MealName)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Meal name must not be whitespace.")
                .MaximumLength(128)
                .WithMessage("Meal name must not exceed 128 characters.");
        });
    }
}
