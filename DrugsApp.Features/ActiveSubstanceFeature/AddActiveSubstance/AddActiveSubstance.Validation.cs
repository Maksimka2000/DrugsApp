using FluentValidation;

namespace DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;

/// <summary>
/// Validator for AddActiveSubstance request payload.
/// </summary>
public sealed class AddActiveSubstanceRequestValidator : AbstractValidator<AddActiveSubstanceRequest>
{
    public AddActiveSubstanceRequestValidator()
    {
        RuleFor(x => x.ActiveSubstanceName)
            .NotEmpty()
            .WithMessage("Active substance name is required.")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Active substance name must not be whitespace.")
            .MaximumLength(200)
            .WithMessage("Active substance name must not exceed 200 characters.");

        When(x => x.OralMedicationIntakeGuidelines is not null, () =>
        {
            RuleFor(x => x.OralMedicationIntakeGuidelines!.FoodIntakeType)
                .IsInEnum()
                .WithMessage("Invalid food intake type value.");

            When(
                x => x.OralMedicationIntakeGuidelines!.WaitBeforeMinutes is not null,
                () => RuleFor(x => x.OralMedicationIntakeGuidelines!.WaitBeforeMinutes)
                    .InclusiveBetween(0, 1440)
                    .WithMessage("WaitBeforeMinutes must be between 0 and 1440."));

            When(
                x => x.OralMedicationIntakeGuidelines!.WaitAfterMinutes is not null,
                () => RuleFor(x => x.OralMedicationIntakeGuidelines!.WaitAfterMinutes)
                    .InclusiveBetween(0, 1440)
                    .WithMessage("WaitAfterMinutes must be between 0 and 1440."));

            When(
                x => !string.IsNullOrWhiteSpace(x.OralMedicationIntakeGuidelines!.MealNotes),
                () => RuleFor(x => x.OralMedicationIntakeGuidelines!.MealNotes)
                    .MaximumLength(1000)
                    .WithMessage("Meal notes must not exceed 1000 characters."));

            When(
                x => !string.IsNullOrWhiteSpace(x.OralMedicationIntakeGuidelines!.AvoidFoodDrug),
                () => RuleFor(x => x.OralMedicationIntakeGuidelines!.AvoidFoodDrug)
                    .MaximumLength(1000)
                    .WithMessage("AvoidFoodDrug must not exceed 1000 characters."));

            When(
                x => !string.IsNullOrWhiteSpace(x.OralMedicationIntakeGuidelines!.AvoidFoodTherapy),
                () => RuleFor(x => x.OralMedicationIntakeGuidelines!.AvoidFoodTherapy)
                    .MaximumLength(1000)
                    .WithMessage("AvoidFoodTherapy must not exceed 1000 characters."));

            When(
                x => !string.IsNullOrWhiteSpace(x.OralMedicationIntakeGuidelines!.LiquidNotes),
                () => RuleFor(x => x.OralMedicationIntakeGuidelines!.LiquidNotes)
                    .MaximumLength(1000)
                    .WithMessage("Liquid notes must not exceed 1000 characters."));
        });
    }
}


