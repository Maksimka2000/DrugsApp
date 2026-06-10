using System;
using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.CreateDrugAllergy;

/// <summary>
/// Validator for creating drug allergies.
/// </summary>
public sealed class CreateDrugAllergyRequestValidator : AbstractValidator<CreateDrugAllergyRequest>
{
    public CreateDrugAllergyRequestValidator()
    {
        RuleFor(request => request.Source)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(source => source.Equals("database", StringComparison.OrdinalIgnoreCase) ||
                            source.Equals("custom", StringComparison.OrdinalIgnoreCase))
            .WithMessage("source must be database or custom.");

        When(request => request.DrugId is not null, () =>
        {
            RuleFor(request => request.DrugId!.Value)
                .NotEqual(Guid.Empty);
        });

        When(request => !string.IsNullOrWhiteSpace(request.CustomDrugName), () =>
        {
            RuleFor(request => request.CustomDrugName!)
                .NotEmpty()
                .MaximumLength(256);
        });

        When(request => !string.IsNullOrWhiteSpace(request.Note), () =>
        {
            RuleFor(request => request.Note!)
                .MaximumLength(1000);
        });

        RuleFor(request => request)
            .Must(request =>
                request.DrugId is not null && string.IsNullOrWhiteSpace(request.CustomDrugName) ||
                request.DrugId is null && !string.IsNullOrWhiteSpace(request.CustomDrugName))
            .WithMessage("Exactly one of drugId or customDrugName must be provided.");
    }
}
