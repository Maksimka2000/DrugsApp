using System;
using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.CreateSupplementAllergy;

public sealed class CreateSupplementAllergyRequestValidator : AbstractValidator<CreateSupplementAllergyRequest>
{
    public CreateSupplementAllergyRequestValidator()
    {
        RuleFor(r => r.Source).Cascade(CascadeMode.Stop).NotEmpty()
            .Must(s => s.Equals("database", StringComparison.OrdinalIgnoreCase) || s.Equals("custom", StringComparison.OrdinalIgnoreCase))
            .WithMessage("source must be database or custom.");

        When(r => r.SupplementId is not null, () => RuleFor(r => r.SupplementId!.Value).NotEqual(Guid.Empty));
        When(r => !string.IsNullOrWhiteSpace(r.CustomSupplementName), () => RuleFor(r => r.CustomSupplementName!).NotEmpty().MaximumLength(256));
        When(r => !string.IsNullOrWhiteSpace(r.Note), () => RuleFor(r => r.Note!).MaximumLength(1000));

        RuleFor(r => r)
            .Must(r => r.SupplementId is not null && string.IsNullOrWhiteSpace(r.CustomSupplementName) ||
                       r.SupplementId is null && !string.IsNullOrWhiteSpace(r.CustomSupplementName))
            .WithMessage("Exactly one of supplementId or customSupplementName must be provided.");
    }
}
