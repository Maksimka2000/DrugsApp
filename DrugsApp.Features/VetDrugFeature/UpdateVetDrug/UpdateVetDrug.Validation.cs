using FluentValidation;

namespace DrugsApp.Features.VetDrugFeature.UpdateVetDrug;

/// <summary>
/// Validator for UpdateVetDrug request with Optional field support.
/// </summary>
public sealed class UpdateVetDrugRequestValidator : AbstractValidator<UpdateVetDrugRequest>
{
    public UpdateVetDrugRequestValidator()
    {
        RuleFor(x => x.CommercialName)
            .NotNull()
            .WithMessage("CommercialName is required.");

        RuleFor(x => x.CommercialName!)
            .NotEmpty()
            .WithMessage("CommercialName cannot be empty.")
            .MaximumLength(100)
            .WithMessage("CommercialName must not exceed 100 characters.");
    }
}

