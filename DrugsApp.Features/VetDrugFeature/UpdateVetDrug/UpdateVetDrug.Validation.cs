using FluentValidation;

namespace DrugsApp.Features.VetDrugFeature.UpdateVetDrug;

/// <summary>
/// Validator for UpdateVetDrug request with Optional field support.
/// </summary>
public sealed class UpdateVetDrugRequestValidator : AbstractValidator<UpdateVetDrugRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateVetDrugRequestValidator"/> class.
    /// </summary>
    public UpdateVetDrugRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.CommercialName), () => RuleFor(x => x.CommercialName)
            .MaximumLength(200)
            .WithMessage("CommercialName must not exceed 200 characters."));
    }
}

