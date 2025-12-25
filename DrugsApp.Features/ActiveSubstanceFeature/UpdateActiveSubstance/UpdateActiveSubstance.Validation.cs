using FluentValidation;

namespace DrugsApp.Features.ActiveSubstanceFeature.UpdateActiveSubstance;

/// <summary>
/// Validator for UpdateActiveSubstance request payload.
/// </summary>
public sealed class UpdateActiveSubstanceRequestValidator : AbstractValidator<UpdateActiveSubstanceRequest>
{
    public UpdateActiveSubstanceRequestValidator()
    {
        When(x => x.ActiveSubstanceName is not null, () => RuleFor(x => x.ActiveSubstanceName)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Active substance name must not be empty or whitespace.")
            .MaximumLength(200)
            .WithMessage("Active substance name must not exceed 200 characters."));

        When(x => x.FoodIntakeType is not null, () => RuleFor(x => x.FoodIntakeType)
            .IsInEnum()
            .WithMessage("Invalid food intake type value."));

        When(x => x.WaitBeforeMinutes is not null && x.ClearWaitBeforeMinutes != true, () => RuleFor(x => x.WaitBeforeMinutes)
            .InclusiveBetween(0, 1440)
            .WithMessage("WaitBeforeMinutes must be between 0 and 1440."));

        When(x => x.WaitAfterMinutes is not null && x.ClearWaitAfterMinutes != true, () => RuleFor(x => x.WaitAfterMinutes)
            .InclusiveBetween(0, 1440)
            .WithMessage("WaitAfterMinutes must be between 0 and 1440."));

        When(x => !string.IsNullOrWhiteSpace(x.MealNotes) && x.ClearMealNotes != true, () => RuleFor(x => x.MealNotes)
            .MaximumLength(1000)
            .WithMessage("Meal notes must not exceed 1000 characters."));

        When(x => !string.IsNullOrWhiteSpace(x.AvoidFoodDrug) && x.ClearAvoidFoodDrug != true, () => RuleFor(x => x.AvoidFoodDrug)
            .MaximumLength(1000)
            .WithMessage("AvoidFoodDrug must not exceed 1000 characters."));

        When(x => !string.IsNullOrWhiteSpace(x.AvoidFoodTherapy) && x.ClearAvoidFoodTherapy != true, () => RuleFor(x => x.AvoidFoodTherapy)
            .MaximumLength(1000)
            .WithMessage("AvoidFoodTherapy must not exceed 1000 characters."));

        When(x => !string.IsNullOrWhiteSpace(x.LiquidNotes) && x.ClearLiquidNotes != true, () => RuleFor(x => x.LiquidNotes)
            .MaximumLength(1000)
            .WithMessage("Liquid notes must not exceed 1000 characters."));
    }
}


