using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.ActiveSubstanceFeature.AddOralMedicationIntakeGuidelines;

/// <summary>
/// Validator for AddOralMedicationIntakeGuidelines request payload.
/// </summary>
public sealed class AddOralMedicationIntakeGuidelinesRequestValidator
    : AbstractValidator<AddOralMedicationIntakeGuidelinesRequest>
{
    public AddOralMedicationIntakeGuidelinesRequestValidator()
    {
        RuleFor(x => x.FoodIntakeType)
            .IsInEnum()
            .WithMessage("Invalid food intake type value.");

        When(x => x.WaitBeforeMinutes is not null, () => RuleFor(x => x.WaitBeforeMinutes)
            .InclusiveBetween(0, 1440)
            .WithMessage("WaitBeforeMinutes must be between 0 and 1440."));

        When(x => x.WaitAfterMinutes is not null, () => RuleFor(x => x.WaitAfterMinutes)
            .InclusiveBetween(0, 1440)
            .WithMessage("WaitAfterMinutes must be between 0 and 1440."));

        When(x => !string.IsNullOrWhiteSpace(x.MealNotes), () => RuleFor(x => x.MealNotes)
            .MaximumLength(1000)
            .WithMessage("Meal notes must not exceed 1000 characters."));

        When(x => !string.IsNullOrWhiteSpace(x.AvoidFoodDrug), () => RuleFor(x => x.AvoidFoodDrug)
            .MaximumLength(1000)
            .WithMessage("AvoidFoodDrug must not exceed 1000 characters."));

        When(x => !string.IsNullOrWhiteSpace(x.AvoidFoodTherapy), () => RuleFor(x => x.AvoidFoodTherapy)
            .MaximumLength(1000)
            .WithMessage("AvoidFoodTherapy must not exceed 1000 characters."));

        When(x => !string.IsNullOrWhiteSpace(x.LiquidNotes), () => RuleFor(x => x.LiquidNotes)
            .MaximumLength(1000)
            .WithMessage("Liquid notes must not exceed 1000 characters."));
    }
}


