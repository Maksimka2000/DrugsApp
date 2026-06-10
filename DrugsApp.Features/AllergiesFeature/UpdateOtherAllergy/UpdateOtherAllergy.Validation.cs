using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.UpdateOtherAllergy;

public sealed class UpdateOtherAllergyRequestValidator : AbstractValidator<UpdateOtherAllergyRequest>
{
    public UpdateOtherAllergyRequestValidator()
    {
        RuleFor(r => r)
            .Must(r => !string.IsNullOrWhiteSpace(r.AllergyName) || r.Note is not null || r.ClearNote == true)
            .WithMessage("At least one of allergyName, note, or clearNote must be provided.");

        When(r => !string.IsNullOrWhiteSpace(r.AllergyName), () => RuleFor(r => r.AllergyName!).NotEmpty().MaximumLength(256));
        When(r => !string.IsNullOrWhiteSpace(r.Note), () => RuleFor(r => r.Note!).MaximumLength(1000));
    }
}
