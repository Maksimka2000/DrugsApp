using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.CreateOtherAllergy;

public sealed class CreateOtherAllergyRequestValidator : AbstractValidator<CreateOtherAllergyRequest>
{
    public CreateOtherAllergyRequestValidator()
    {
        RuleFor(r => r.AllergyName).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(256);
        When(r => !string.IsNullOrWhiteSpace(r.Note), () => RuleFor(r => r.Note!).MaximumLength(1000));
    }
}
