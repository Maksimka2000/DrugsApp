using FluentValidation;

namespace DrugsApp.Features.VetDrugFeature.UpdateVetDrug;

/// <summary>
/// Validator for UpdateVetDrug request with Optional field support.
/// </summary>
public sealed class UpdateVetDrugRequestValidator : AbstractValidator<UpdateVetDrugRequest>
{
    public UpdateVetDrugRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.CommercialName), () => RuleFor(x => x.CommercialName)
            .MaximumLength(200)
            .WithMessage("CommercialName must not exceed 200 characters."));
    }
}

