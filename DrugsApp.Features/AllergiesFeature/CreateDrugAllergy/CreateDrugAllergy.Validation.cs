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

        When(request => request.DrugFormId is not null, () =>
        {
            RuleFor(request => request.DrugFormId!.Value)
                .NotEqual(Guid.Empty);
        });

        When(request => !string.IsNullOrWhiteSpace(request.CustomDrugName), () =>
        {
            RuleFor(request => request.CustomDrugName!)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(request => request.CustomActiveSubstances!)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("customActiveSubstances must be provided.")
                .MaximumLength(300);

            RuleFor(request => request.CustomFormulationName!)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("customFormulationName must be provided.")
                .MaximumLength(300);

            RuleFor(request => request.CustomDosage!)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("customDosage must be provided.")
                .MaximumLength(200);
        });

        When(request => !string.IsNullOrWhiteSpace(request.Note), () =>
        {
            RuleFor(request => request.Note!)
                .MaximumLength(1000);
        });

        RuleFor(request => request)
            .Must(request =>
                request.DrugFormId is not null && string.IsNullOrWhiteSpace(request.CustomDrugName) ||
                request.DrugFormId is null && !string.IsNullOrWhiteSpace(request.CustomDrugName))
            .WithMessage("Exactly one of drugFormId or customDrugName must be provided.");
    }
}
