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
            .Must(opt => !opt.HasValue || !string.IsNullOrWhiteSpace(opt.Value))
            .WithMessage("CommercialName cannot be empty when provided.");

        RuleFor(x => x.CommercialName)
            .Must(opt => !opt.HasValue || opt.Value.Length <= 100)
            .WithMessage("CommercialName must not exceed 100 characters.");
    }
}

