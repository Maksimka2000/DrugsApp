using System;
using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.CreateFoodAllergy;

public sealed class CreateFoodAllergyRequestValidator : AbstractValidator<CreateFoodAllergyRequest>
{
    public CreateFoodAllergyRequestValidator()
    {
        RuleFor(r => r.Source).Cascade(CascadeMode.Stop).NotEmpty()
            .Must(s => s.Equals("database", StringComparison.OrdinalIgnoreCase) || s.Equals("custom", StringComparison.OrdinalIgnoreCase))
            .WithMessage("source must be database or custom.");

        When(r => r.FoodId is not null, () => RuleFor(r => r.FoodId!.Value).NotEqual(Guid.Empty));
        When(r => !string.IsNullOrWhiteSpace(r.CustomFoodName), () => RuleFor(r => r.CustomFoodName!).NotEmpty().MaximumLength(256));
        When(r => !string.IsNullOrWhiteSpace(r.Note), () => RuleFor(r => r.Note!).MaximumLength(1000));

        RuleFor(r => r)
            .Must(r => r.FoodId is not null && string.IsNullOrWhiteSpace(r.CustomFoodName) ||
                       r.FoodId is null && !string.IsNullOrWhiteSpace(r.CustomFoodName))
            .WithMessage("Exactly one of foodId or customFoodName must be provided.");
    }
}
